// ================= Aurora Framework =================

// Define types for the virtual DOM.
export type VNodeChild = VNode | string | number;
export interface VNode {
  tag: string;
  props: Record<string, string>;
  children: VNodeChild[];
}

// Define a type for reactive states.
export interface Reactive<T> extends T {
  subscribe(fn: () => void): void;
  unsubscribe(fn: () => void): void;
}

// Aurora module.
export const Aurora = (() => {
  // Create a virtual DOM element.
  function createElement(
    tag: string,
    props: Record<string, string> | null,
    ...children: VNodeChild[]
  ): VNode {
    return { tag, props: props || {}, children };
  }

  // Basic reactive state.
  function reactive<T extends object>(initialState: T): Reactive<T> {
    const state: T & Partial<Reactive<T>> = { ...initialState };
    const listeners = new Set<() => void>();
    state.subscribe = (fn: () => void): void => listeners.add(fn);
    state.unsubscribe = (fn: () => void): void => listeners.delete(fn);

    return new Proxy(state, {
      set(target: T, prop: string | symbol, value: any): boolean {
        (target as any)[prop] = value;
        for (const listener of listeners) {
          listener();
        }
        return true;
      }
    }) as Reactive<T>;
  }

  // Render: converts a virtual DOM tree into real DOM.
  function render(vnode: VNodeChild, container: HTMLElement): void {
    if (typeof vnode === "string" || typeof vnode === "number") {
      container.textContent = vnode.toString();
      return;
    }
    const el = document.createElement(vnode.tag);
    for (const key in vnode.props) {
      el.setAttribute(key, vnode.props[key]);
    }
    vnode.children.forEach(child => render(child, el));
    container.innerHTML = "";
    container.appendChild(el);
  }

  return { createElement, reactive, render };
})();

// ================= Helper Functions =================

// Global log array with timestamps.
let executionLogs: string[] = [];

function logStep(msg: string): void {
  const timestamp = new Date().toISOString();
  const logMsg = `[${timestamp}] ${msg}`;
  executionLogs.push(logMsg);
  console.log(logMsg);
}

// Global cache helper.
const globalCache = new Map<string, any>();

function cacheWrap<T extends (...args: any[]) => any>(name: string, fn: T): T {
  return ((...args: any[]) => {
    const key = name + ":" + JSON.stringify(args);
    if (globalCache.has(key)) {
      logStep(`[Cache] ${name} with args ${JSON.stringify(args)}`);
      return globalCache.get(key);
    }
    const res = fn(...args);
    globalCache.set(key, res);
    logStep(`[Compute] ${name} with args ${JSON.stringify(args)}`);
    return res;
  }) as T;
}

// ================= VirtualS3 Module =================

namespace VirtualS3Module {
  // Each bucket is a reactive state.
  const buckets: Record<string, any> = {};

  export function getBucket<T = any>(bucketName: string): Reactive<Record<string, T>> {
    if (!buckets[bucketName]) {
      buckets[bucketName] = Aurora.reactive<Record<string, T>>({});
    }
    return buckets[bucketName];
  }

  export function putObject<T = any>(bucket: string, key: string, value: T): T {
    const b = getBucket<T>(bucket);
    b[key] = value;
    return value;
  }

  export function getObject<T = any>(bucket: string, key: string): T | undefined {
    const b = getBucket<T>(bucket);
    return b[key];
  }

  export function updateObject<T = any>(bucket: string, key: string, value: T): T {
    const b = getBucket<T>(bucket);
    b[key] = value;
    return value;
  }
}

const VirtualS3 = {
  putObject: VirtualS3Module.putObject,
  getObject: VirtualS3Module.getObject,
  updateObject: VirtualS3Module.updateObject,
  getBucket: VirtualS3Module.getBucket,
};

// ================= VirtualAPI (Using VirtualS3) =================

export class VirtualAPI {
  static allocateToken(funcName: string, token: string): string {
    return VirtualS3.putObject("tokens", funcName, token);
  }
  static getToken(funcName: string): string | undefined {
    return VirtualS3.getObject("tokens", funcName);
  }
  static verifyToken(funcName: string, token: string): boolean {
    return VirtualS3.getObject("tokens", funcName) === token;
  }
  static saveVariable(varName: string, value: any): any {
    return VirtualS3.putObject("variables", varName, value);
  }
  static getVariable(varName: string): any {
    return VirtualS3.getObject("variables", varName);
  }
}

// ================= FakeMongoDB Implementation =================

export class FakeMongoDB {
  private db: Record<string, any[]> = {};

  createCollection(name: string): void {
    if (!this.db[name]) this.db[name] = [];
  }

  insert(collection: string, doc: any): any {
    this.createCollection(collection);
    this.db[collection].push(doc);
    return doc;
  }

  find(collection: string, query: Record<string, any>): any[] {
    this.createCollection(collection);
    return this.db[collection].filter(doc =>
      Object.keys(query).every(key => doc[key] === query[key])
    );
  }

  update(collection: string, query: Record<string, any>, updateObj: Record<string, any>): number {
    this.createCollection(collection);
    let count = 0;
    this.db[collection] = this.db[collection].map(doc => {
      if (Object.keys(query).every(key => doc[key] === query[key])) {
        count++;
        return { ...doc, ...updateObj };
      }
      return doc;
    });
    return count;
  }

  remove(collection: string, query: Record<string, any>): number {
    this.createCollection(collection);
    const origLen = this.db[collection].length;
    this.db[collection] = this.db[collection].filter(doc =>
      !Object.keys(query).every(key => doc[key] === query[key])
    );
    return origLen - this.db[collection].length;
  }
}

const fakeDB = new FakeMongoDB();

// ================= CUSTOM CRYPTO LIBRARY (SHA-256) =================

export const CryptoLib = (() => {
  function rightRotate(value: number, amount: number): number {
    return (value >>> amount) | (value << (32 - amount));
  }

  function sha256(ascii: string): string {
    const mathPow = Math.pow;
    const maxWord = mathPow(2, 32);
    let result = "";
    const words: number[] = [];
    const asciiBitLength = ascii.length * 8;
    let hash: number[] = [];
    let k: number[] = [];
    let primeCounter = 0;
    const isComposite: Record<number, boolean> = {};

    for (let candidate = 2; primeCounter < 64; candidate++) {
      if (!isComposite[candidate]) {
        for (let i = candidate * candidate; i < 313; i += candidate) {
          isComposite[i] = true;
        }
        hash[primeCounter] = ((mathPow(candidate, 0.5)) * maxWord) | 0;
        k[primeCounter++] = ((mathPow(candidate, 1 / 3)) * maxWord) | 0;
      }
    }

    ascii += "\x80";
    while (ascii.length % 4 !== 0) ascii += "\x00";

    for (let i = 0; i < ascii.length; i += 4) {
      const j =
        (ascii.charCodeAt(i) << 24) |
        (ascii.charCodeAt(i + 1) << 16) |
        (ascii.charCodeAt(i + 2) << 8) |
        ascii.charCodeAt(i + 3);
      words.push(j);
    }

    while (words.length % 16 !== 14) words.push(0);
    words.push((asciiBitLength / maxWord) | 0);
    words.push(asciiBitLength | 0);

    for (let j = 0; j < words.length; ) {
      const w = words.slice(j, j += 16);
      const oldHash = hash.slice(0);
      for (let i = 0; i < 64; i++) {
        const s0 = i < 16 ? w[i] : (rightRotate(w[i - 15], 7) ^ rightRotate(w[i - 15], 18) ^ (w[i - 15] >>> 3));
        const s1 = i < 16 ? 0 : (rightRotate(w[i - 2], 17) ^ rightRotate(w[i - 2], 19) ^ (w[i - 2] >>> 10));
        w[i] = i < 16 ? w[i] : (((w[i - 16] + s0 + w[i - 7] + s1) | 0));
        const a = hash[0], e = hash[4];
        const temp1 = (hash[7] +
          (rightRotate(e, 6) ^ rightRotate(e, 11) ^ rightRotate(e, 25)) +
          ((e & hash[5]) ^ ((~e) & hash[6])) + k[i] + w[i]) | 0;
        const temp2 = ((rightRotate(a, 2) ^ rightRotate(a, 13) ^ rightRotate(a, 22)) +
          ((a & hash[1]) ^ (a & hash[2]) ^ (hash[1] & hash[2]))) | 0;
        hash = [ (temp1 + temp2) | 0 ].concat(hash);
        hash[4] = (hash[4] + temp1) | 0;
        hash.pop();
      }
      for (let i = 0; i < 8; i++) {
        hash[i] = (hash[i] + oldHash[i]) | 0;
      }
    }
    for (let i = 0; i < 8; i++) {
      for (let j = 3; j >= 0; j--) {
        const b = (hash[i] >> (j * 8)) & 255;
        result += (b < 16 ? "0" : "") + b.toString(16);
      }
    }
    return result;
  }

  return {
    createHash: function (algorithm: string) {
      if (algorithm !== "sha256") throw new Error("Unsupported algorithm: " + algorithm);
      let data = "";
      return {
        update: function (chunk: string) {
          data += chunk;
          return this;
        },
        digest: function (encoding: string) {
          const hash = sha256(data);
          if (encoding === "hex") return hash;
          else if (encoding === "binary") {
            let binary = "";
            for (let i = 0; i < hash.length; i += 2) {
              binary += String.fromCharCode(parseInt(hash.substr(i, 2), 16));
            }
            return binary;
          } else throw new Error("Unsupported encoding: " + encoding);
        }
      };
    }
  };
})();

// ----- Dummy JWT Functions (Simulated) -----

function generateJWT(payload: Record<string, any>, secret: string): string {
  const base64Payload = btoa(JSON.stringify(payload))
    .replace(/\+/g, "-")
    .replace(/\//g, "_")
    .replace(/=+$/, "");
  return "JWT:" + base64Payload + ":" + secret;
}

function verifyJWT(token: string, secret: string): boolean {
  return token.endsWith(":" + secret);
}

// ----- Language Source Codes as Strings -----
export const langSources: Record<string, string> = {
  csharp: `
      // C#-like tokenization code (pseudo-code)
      public static string Tokenize(string input) {
          char[] arr = input.ToCharArray();
          Array.Reverse(arr);
          return "CSharpToken:" + new string(arr);
      }
  `,
  erlang: `
      %% Erlang tokenization (pseudo-code)
      tokenize(Input) ->
          lists:concat(["ErlangToken:<", Input, ">"]).
  `,
  nim: `
      # Nim-like tokenization (pseudo-code)
      proc tokenize(input: string): string =
          result = "NimToken:" & input.split("").mapIt($it.ord).join("")
  `
};

// ----- Conversion Architecture for Languages -----
// Returns a function that tokenizes a string.
function convertLangToJS(lang: string, sourceCode: string): (input: string) => string {
  let jsCode = sourceCode;
  if (lang === "csharp") {
    jsCode = jsCode.replace(/public\s+static\s+string\s+Tokenize\s*\(\s*string\s+(\w+)\s*\)/g, "function Tokenize($1)");
    jsCode = jsCode.replace(/char\[\]\s+/g, "let ");
    jsCode = jsCode.replace(/\.ToCharArray\(\)/g, ".split('')");
    jsCode = jsCode.replace(/Array\.Reverse\((\w+)\)/g, "$1.reverse()");
    jsCode = jsCode.replace(/new\s+string\((\w+)\)/g, "$1.join('')");
    jsCode = jsCode.replace(/;/g, "");
    jsCode = jsCode.trim();
    return eval("(" + jsCode + ")") as (input: string) => string;
  } else if (lang === "erlang") {
    jsCode = jsCode.replace(/%%.*\n/g, " ");
    jsCode = jsCode.replace(/\n/g, " ");
    let match = jsCode.match(/lists:concat\(\[(.*?)\]\)/);
    if (!match) throw new Error("Failed to convert Erlang code");
    let parts = match[1].split(/\s*,\s*/).map(p => p.trim());
    parts = parts.map(part => {
      if (part.startsWith('"') || part.startsWith("'")) return part;
      else return part;
    });
    const retStmt = "return " + parts.join(" + ") + ";";
    const finalCode = "function tokenize(Input) { " + retStmt + " }";
    return eval("(" + finalCode + ")") as (input: string) => string;
  } else if (lang === "nim") {
    jsCode = jsCode.replace(/#.*\n/g, " ");
    jsCode = jsCode.replace(/&/g, "+");
    jsCode = jsCode.replace(/\.mapIt\(\$it\.ord\)/g, ".map(ch => ch.charCodeAt(0))");
    jsCode = jsCode.replace(
      /proc\s+(\w+)\(input:\s*string\):\s*string\s*=\s*result\s*=\s*(.*)/,
      "function $1(input) { return $2; }"
    );
    return eval("(" + jsCode + ")") as (input: string) => string;
  } else {
    throw new Error("Unknown language: " + lang);
  }
}

export const tokenizeCSharp = convertLangToJS("csharp", langSources.csharp);
export const tokenizeErlang = convertLangToJS("erlang", langSources.erlang);
export const tokenizeNim = convertLangToJS("nim", langSources.nim);

// ----- Simulated GPU Computation with JWT and Tokenization -----
const SECRET = "S3cr3t!@#_V@lUe";

const gpuCompute = cacheWrap("gpuCompute", function(input: string): string {
  let acc = 0;
  for (let i = 0, len = input.length; i < len; i++) {
    let local = input.charCodeAt(i);
    for (let j = 0; j < 1000; j++) {
      local = (local * 31 + j) % 1000003;
    }
    acc = (acc + local) % 1000003;
  }
  const computedHex = acc.toString(16).padStart(6, "0");
  const csToken = tokenizeCSharp("verify:" + input);
  const erlangToken = tokenizeErlang("verify:" + input);
  const nimToken = tokenizeNim("verify:" + input);
  const jwtPayload = { gpu: computedHex, csharp: csToken, erlang: erlangToken, nim: nimToken };
  const jwtToken = generateJWT(jwtPayload, SECRET);
  if (!verifyJWT(jwtToken, SECRET)) throw new Error("JWT verification failed in GPU compute");
  logStep(`gpuCompute: computed ${computedHex}`);
  return computedHex + "|" + jwtToken;
});

// ----- Wrap _cryptoToken -----
const _cryptoTokenCached = cacheWrap("_cryptoToken", function(funcName: string): string {
  const baseToken = tokenizeCSharp("verify:" + funcName + SECRET) +
                    tokenizeErlang("verify:" + funcName + SECRET) +
                    tokenizeNim("verify:" + funcName + SECRET);
  const gpuVal = gpuCompute("verify:" + funcName + SECRET);
  const combined = gpuVal + baseToken;
  const token = CryptoLib.createHash('sha256').update(combined).digest('hex');
  logStep(`_cryptoToken: ${funcName} generated token ${token}`);
  return token;
});

function _cryptoToken(funcName: string): string {
  return _cryptoTokenCached(funcName);
}

function _cryptoVerifyLink(funcName: string): string {
  logStep(`_cryptoVerifyLink: Executing ${funcName}`);
  const token = _cryptoToken(funcName);
  const existing = VirtualAPI.getToken(funcName);
  if (existing === undefined) VirtualAPI.allocateToken(funcName, token);
  else if (!VirtualAPI.verifyToken(funcName, token))
    throw new Error("Crypto verification failed for " + funcName);
  return token;
}

// ----- Common Decoy Function -----
const _commonLinkCached = cacheWrap("_commonLink", function(input: string): string {
  const transformed = input.split("").reverse().join("") + "x";
  VirtualAPI.saveVariable("commonLink:" + input, transformed);
  logStep(`_commonLink: transformed "${input}" to "${transformed}"`);
  return transformed;
});
function _commonLink(input: string): string {
  return _commonLinkCached(input);
}

// ----- Hex Decoder Function -----
function _h(hex: string): string {
  _cryptoVerifyLink("hexDecoder");
  let out = "";
  for (let i = 0, len = hex.length; i < len; i += 2) {
    _cryptoVerifyLink("hexLoop");
    out += String.fromCharCode(parseInt(hex.substr(i, 2), 16));
  }
  logStep(`_h: decoded ${hex} to "${out}"`);
  return out;
}

// ----- Decoded Identifiers -----
const _ids = {
  console: _h("636f6e736f6c65"), // "console"
  log: _h("6c6f67"),            // "log"
  msg: _h("776879")             // "why"
};

// ----- Decoy Class -----
class _C {
  public val: number;
  [key: string]: any;

  constructor(val: number) {
    _cryptoVerifyLink("DecoyConstructor");
    this.val = val;
    this[_h("6964")] = Math.random();
    logStep(`_C: Constructor called with val=${val}`);
  }

  *gen(): Generator<number> {
    _cryptoVerifyLink("DecoyGen");
    for (let i = 0; i < 3; i++) {
      _cryptoVerifyLink("DecoyGenLoop");
      logStep(`_C.gen: yielding ${i * this.val}`);
      yield i * this.val;
    }
  }

  async asyncM(): Promise<string> {
    _cryptoVerifyLink("DecoyAsyncM");
    logStep(`_C.asyncM: called`);
    return Promise.resolve("asyncDecoy");
  }

  method(): number {
    _cryptoVerifyLink("DecoyMethod");
    logStep(`_C.method: returning ${this.val}`);
    return this.val;
  }
}

const _ci = new _C(42);
const _genVals: number[] = [..._ci.gen()];
_ci.asyncM().then(() => { _cryptoVerifyLink("DecoyAsyncThen"); });

// ----- Various Decoy Constructs -----
const _tObj = { num: 100 };
const _px = new Proxy(_tObj, {
  get(t, k: string | symbol) {
    _cryptoVerifyLink("ProxyGet");
    return k in t ? t[k as keyof typeof t] : 42;
  }
});
const _sym = Symbol("obf");
const _map = new Map<number, string>([[1, "one"], [2, "two"]]);
const _set = new Set<number>([3, 4, 5]);
const _wm = new WeakMap<object, any>();
const _ws = new WeakSet<object>();

_cryptoVerifyLink("TemplateLiteral");
const _nums: number[] = [1, 2, 3, ...[4, 5, 6]];
const { a: _aVal, b: _bVal, ..._rest } = { a: 10, b: 20, c: 30, d: 40 };
const _tpl: string = `Tpl_${_aVal}_${_bVal}_${_rest.c}`;

let _withRes: string;
const _withObj = { p: "with", q: "stmt" };
{
  _cryptoVerifyLink("withStmt");
  _withRes = _withObj.p + "_" + _withObj.q;
}
const _opt = _withObj?.["z"] ?? _cryptoVerifyLink("optChain");

_cryptoVerifyLink("TypedArray");
const _ta = new Uint8Array([87, 72, 89]);
const _taStr = String.fromCharCode(..._ta);
const _big = BigInt("9876543210123456789");
const _comp = { [("c" + "omputed")]: 777 };
const _compVal = Reflect.get(_comp, "computed");

_cryptoVerifyLink("beforeDynFunc");
let _dynSrc = "";
_dynSrc += "return (function(){";
_dynSrc += " _cryptoVerifyLink('dynInner'); var s = '" + _ids.msg + "';";
_dynSrc += "return s;";
_dynSrc += "})();";
const _dynF = new Function("_cryptoVerifyLink", _dynSrc);
const _dynRes = _dynF(_cryptoVerifyLink);
_cryptoVerifyLink("afterDynFunc");
const _ev = eval("(function(){ _cryptoVerifyLink('evalFunc'); return String.fromCharCode(119,104,121); })()");

let _hr = 0;
_cryptoVerifyLink("HeavyCompStart");
for (let i = 0; i < 100; i++) {
  for (let j = 0; j < 100; j++) {
    _cryptoVerifyLink("HeavyLoop");
    _hr += Math.sin(i) * Math.cos(j) + Math.sqrt(i + j);
  }
}

function _iterHeavy(n: number, acc: number): number {
  _cryptoVerifyLink("iterHeavy");
  while (n > 0) {
    _cryptoVerifyLink("iterHeavyLoop");
    acc += Math.tan(n % 10);
    n--;
  }
  return acc;
}

const _recHeavy = _iterHeavy(100, 0);
let _str = "obfuscate".repeat(10);
for (let i = 0; i < 100; i++) {
  _cryptoVerifyLink("StringOp");
  _str = _str.split("").reverse().join("") + _str.slice(0, 5);
}
const _sum = ((...nums: number[]): number => {
  _cryptoVerifyLink("ArrowSum");
  return nums.reduce((a, b) => a + b, 0);
})(1, 2, 3, 4, 5);
for (let i = 0; i < 20; i++) {
  ((j: number) => {
    _cryptoVerifyLink("LoopSquare");
    let _sq = j * j;
  })(i);
}
const _joined = "w" + "h" + "y";

_cryptoVerifyLink("DynObj");
const _dynObj: { x?: any; y?: any; z?: any } = { x: undefined, y: undefined, z: undefined };
const _compObj = { [("c" + "ode").toUpperCase()]: _sum };

(function* () {
  _cryptoVerifyLink("GenFunc");
  yield "g1";
  yield "g2";
})();
(async () => {
  _cryptoVerifyLink("AsyncArrow");
  await Promise.resolve("asyncArrow");
})();
const _fStr = "(()=>{ _cryptoVerifyLink('ExtraDyn'); return '" + _ids.msg + "'; })()";
const _fDyn = eval(_fStr);
[
  () => { _cryptoVerifyLink("ExtraDynCall1"); return _fDyn; },
  () => { _cryptoVerifyLink("ExtraDynCall2"); return _dynRes; },
  () => { _cryptoVerifyLink("ExtraDynCall3"); return _joined; }
].forEach(fn => fn());
let _mod = 0;
const _decoys = [
  _tpl,
  _withRes,
  _opt,
  _taStr,
  _dynRes,
  _ev,
  _joined,
  _genVals.join(","),
  String(_big),
  _sum,
  _dynObj.x,
  _compObj.CODE
];
_decoys.forEach(item => {
  try {
    _cryptoVerifyLink("DecoyAggregate");
    _mod += (typeof item === "string" ? item.length : 0) % 7;
  } catch (e) { }
});
const _obfArr = ["alpha", "beta", "gamma", "delta"];
const _obfRes = _obfArr.map((str: string): string => {
  _cryptoVerifyLink("ObfMap");
  let res = "";
  for (let ch of str) {
    _cryptoVerifyLink("ObfMapLoop");
    res += String.fromCharCode(ch.charCodeAt(0) + _mod % 10);
  }
  return res;
});
_obfRes.forEach(val => {
  _cryptoVerifyLink("ObfForEach");
  _mod += val.length % 4;
});
const _api: Record<string, any> = {
  [_h("666574636844617461")]: async function (): Promise<string> {
    _cryptoVerifyLink("apiFetchData");
    return new Promise(resolve => {
      setTimeout(() => { resolve(_h("6f66666c696e652d726573706f6e7365")); }, 100);
    });
  },
  [_h("706f737444617461")]: function (): string {
    _cryptoVerifyLink("apiPostData");
    return _h("6c6f63616c2d64617461");
  },
  [_h("75706461746544617461")]: async function (): Promise<string> {
    _cryptoVerifyLink("apiUpdateData");
    return new Promise(resolve => {
      setTimeout(() => { resolve(_h("757064617465642d6f6b")); }, 150);
    });
  }
};

// ----- Aurora Integration & Rendering -----
if (typeof document !== "undefined") {
  // Clear the DOM.
  document.body.innerHTML = "";
  // Inject custom Tailwind-like styles.
  const style = document.createElement("style");
  style.innerHTML = `
    .bg-gray-100 { background-color: #f7fafc; }
    .text-gray-800 { color: #2d3748; }
    .p-4 { padding: 1rem; }
    .m-2 { margin: 0.5rem; }
    .border { border: 1px solid #e2e8f0; }
    .rounded { border-radius: 0.25rem; }
    .font-mono { font-family: monospace; }
    .text-sm { font-size: 0.875rem; }
    .container { max-width: 800px; margin: auto; }
    .header { font-size: 2rem; font-weight: bold; }
    .section { margin-top: 1rem; }
    .table { width: 100%; border-collapse: collapse; }
    .table th, .table td { border: 1px solid #e2e8f0; padding: 0.5rem; }
    .table th { background: #edf2f7; }
    .centered { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100vh; overflow-y: auto; }
  `;
  document.head.appendChild(style);

  // AuroraApp Component: displays key variables, logs, Aurora tests, comments, and final message.
  function AuroraApp(): VNode {
    return Aurora.createElement("div", { class: "centered container bg-gray-100 p-4 text-gray-800" },
      Aurora.createElement("h1", { class: "header m-2" }, "Aurora Framework Demo"),
      Aurora.createElement("p", { class: "m-2" },
        "This UI is rendered by Aurora. It shows execution logs, VirtualAPI (S3) state, Aurora framework tests, final message, and comments."
      ),
      Aurora.createElement("div", { class: "section m-2" },
        Aurora.createElement("h2", { class: "font-mono text-sm" }, "Decoded Identifiers (_ids):"),
        Aurora.createElement("pre", { class: "font-mono text-sm border rounded p-2" }, JSON.stringify(_ids, null, 2))
      ),
      Aurora.createElement("div", { class: "section m-2" },
        Aurora.createElement("h2", { class: "font-mono text-sm" }, "VirtualAPI Variables (S3 State):"),
        Aurora.createElement("pre", { class: "font-mono text-sm border rounded p-2" }, JSON.stringify(VirtualS3.getBucket("variables"), null, 2))
      ),
      Aurora.createElement("div", { class: "section m-2" },
        Aurora.createElement("h2", { class: "font-mono text-sm" }, "Execution Logs:"),
        Aurora.createElement("pre", { class: "font-mono text-sm border rounded p-2" }, executionLogs.join("\n"))
      ),
      Aurora.createElement("div", { class: "section m-2" },
        Aurora.createElement("h2", { class: "font-mono text-sm" }, "Aurora Framework Tests:"),
        Aurora.createElement("pre", { class: "font-mono text-sm border rounded p-2" }, auroraTestLogs.join("\n"))
      ),
      // Final message section:
      Aurora.createElement("div", { class: "section m-2" },
        Aurora.createElement("h2", { class: "font-mono text-sm" }, "Final Message:"),
        Aurora.createElement("pre", { class: "font-mono text-sm border rounded p-2" }, _ids.msg)
      ),
      Aurora.createElement("div", { class: "section m-2" },
        Aurora.createElement("h2", { class: "font-mono text-sm" }, "Comments:"),
        Aurora.createElement("p", { class: "font-mono text-sm" },
          "1. Advanced crypto and tokenization functions have been executed.\n" +
          "2. A VirtualS3 module holds all reactive states for tokens and variables.\n" +
          "3. Aurora renders this reactive UI, showing execution logs, VirtualS3 state, and Aurora tests.\n" +
          "4. A 100ms delay is added before rendering to capture all logs."
        )
      )
    );
  }

  // Aurora Test Suite for the framework.
  let auroraTestLogs: string[] = [];
  function logAuroraTest(msg: string): void {
    const ts = new Date().toISOString();
    const fullMsg = `[AuroraTest ${ts}] ${msg}`;
    auroraTestLogs.push(fullMsg);
    console.log(fullMsg);
  }
  function testAurora(): boolean {
    try {
      // Test createElement and render.
      const container = document.createElement("div");
      const vnode = Aurora.createElement("div", { id: "test" }, "Hello Aurora");
      Aurora.render(vnode, container);
      if (!container.innerHTML.includes("Hello Aurora")) throw new Error("Aurora.render failed");
      logAuroraTest("Aurora.render: OK");
      // Test reactive state.
      const state = Aurora.reactive({ count: 0 });
      let updated = false;
      state.subscribe(() => { updated = true; });
      state.count = 1;
      if (!updated) throw new Error("Aurora.reactive failed");
      logAuroraTest("Aurora.reactive: OK");
      return true;
    } catch (err: any) {
      logAuroraTest("Aurora test failed: " + err.message);
      throw err;
    }
  }

  // Delay 100ms before rendering the AuroraApp.
  setTimeout(() => {
    try { testAurora(); } catch (e) { }
    Aurora.render(AuroraApp(), document.body);
    logStep("AuroraApp rendered.");
  }, 100);
} else {
  let _final = "";
  for (let i = 0; i < _ids.msg.length; i++) {
    _cryptoVerifyLink("FinalTransform");
    _final += String.fromCharCode(_ids.msg.charCodeAt(i));
  }
  console.log(_final);
}

// ----- Core Test Suite -----
async function runTests(): Promise<boolean> {
  try {
    const dbTest = new FakeMongoDB();
    dbTest.insert("testColl", { id: 1, val: "a" });
    if (dbTest.find("testColl", { id: 1 })[0].val !== "a") throw new Error("FakeMongoDB insert/find failed");
    dbTest.update("testColl", { id: 1 }, { val: "b" });
    if (dbTest.find("testColl", { id: 1 })[0].val !== "b") throw new Error("FakeMongoDB update failed");
    dbTest.remove("testColl", { id: 1 });
    if (dbTest.find("testColl", { id: 1 }).length !== 0) throw new Error("FakeMongoDB remove failed");

    VirtualAPI.allocateToken("testFunc", "token123");
    if (!VirtualAPI.verifyToken("testFunc", "token123")) throw new Error("VirtualAPI token verification failed");
    VirtualAPI.saveVariable("varTest", 42);
    if (VirtualAPI.getVariable("varTest") !== 42) throw new Error("VirtualAPI variable storage failed");

    const hash = CryptoLib.createHash('sha256').update("test").digest('hex');
    if (hash !== "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08") {
      throw new Error("CryptoLib SHA256 test failed: " + hash);
    }

    if (_commonLink("abc") !== "cbax") throw new Error("_commonLink failed");

    const tokenTest = _cryptoToken("testFunc2");
    const expectedToken = CryptoLib.createHash('sha256')
      .update(
        gpuCompute("verify:" + "testFunc2" + SECRET) +
        tokenizeCSharp("verify:" + "testFunc2" + SECRET) +
        tokenizeErlang("verify:" + "testFunc2" + SECRET) +
        tokenizeNim("verify:" + "testFunc2" + SECRET)
      )
      .digest('hex');
    if (tokenTest !== expectedToken) throw new Error("_cryptoToken failed");

    _cryptoVerifyLink("testVerify");
    // Tamper with the token in VirtualS3 for "testVerify"
    VirtualS3.updateObject("tokens", "testVerify", "tampered");
    let errorThrown = false;
    try { _cryptoVerifyLink("testVerify"); } catch (e) { errorThrown = true; }
    if (!errorThrown) throw new Error("_cryptoVerifyLink tampering test failed");

    if (_h("776879") !== "why") throw new Error("_h decoder failed");

    const instance = new _C(3);
    const genValues = [...instance.gen()];
    if (genValues.length !== 3 || genValues[0] !== 0 || genValues[1] !== 3 || genValues[2] !== 6) {
      throw new Error("_C generator test failed");
    }
    const asyncRes = await instance.asyncM();
    if (asyncRes !== "asyncDecoy") throw new Error("_C asyncM test failed");
    if (instance.method() !== 3) throw new Error("_C method test failed");

    const apiFetchRes = await _api[_h("666574636844617461")]();
    if (apiFetchRes !== "offline-response") throw new Error("API fetch test failed");
    const apiPostRes = _api[_h("706f737444617461")]();
    if (apiPostRes !== "local-data") throw new Error("API post test failed");
    const apiUpdateRes = await _api[_h("75706461746544617461")]();
    if (apiUpdateRes !== "updated-ok") throw new Error("API update test failed");

    logStep("All tests passed.");
    return true;
  } catch (err) {
    console.error("Test failed:", err);
    return false;
  }
}

// ----- MAIN CODE EXECUTION -----
async function runMain(): Promise<void> {
  if (typeof document === "undefined") {
    let _final = "";
    for (let i = 0; i < _ids.msg.length; i++) {
      _cryptoVerifyLink("FinalTransform");
      _final += String.fromCharCode(_ids.msg.charCodeAt(i));
    }
    console.log(_final);
  }
}

// Wrap test runner in async IIFE to allow await.
(async () => {
  if (await runTests()) {
    runMain();
  }
})();
