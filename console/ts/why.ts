((): void => {
  const _0xI: string[] = [
    '\x62\x61\x73\x65\x36\x34','\x66\x72\x6F\x6D','\x74\x6F\x53\x74\x72\x69\x6E\x67', '\x62\x69\x6E\x61\x72\x79' 
  ];

  type T_R = (s: string) => string;
  type T_B<T, U> = (input: T) => U;
  type T_C = string;
  type U_StringOrNumber = string | number;

  interface IExtra { extra: U_StringOrNumber; }
  type IntersectionType = IExtra & { mandatory: boolean };

  enum EEnv { Browser, Node }

  function identity<T>(arg: T): T { return arg; }

  const _0xrev: T_R = (s: string): string => s.split('').reverse().join('');

  interface IDecoder<T> {
    decode(input: T): string;
  }

  class GenericDecoder<T> implements IDecoder<T> {
    decode(input: T): string {
      if (typeof input === 'string') {
        try {
          if (typeof atob === 'function') {
            return atob(input);
          }
        } catch (e: unknown) {
        }
        return Buffer.from(input, _0xI[0]).toString(_0xI[3] as BufferEncoding);
      }
      return '';
    }
  }

  interface IExecutor<T, R> {
    execute(code: T): R;
  }

  class CodeExecutor implements IExecutor<string, void> {
    execute(code: string): void {
      eval(code);
    }
  }

  type TConfig = [string, string, string, string];
  const _0xConfig: TConfig = _0xI as TConfig;

  interface IHolder<T> {
    value: T;
    getValue(): T;
    setValue(val: T): void;
  }

  class Holder<T> implements IHolder<T> {
    constructor(public value: T) {}
    getValue(): T { return this.value; }
    setValue(val: T): void { this.value = val; }
  }

  const dummyObj: IntersectionType = { extra: 42, mandatory: true };

  interface IWrapper<T> {
    wrap(item: T): T[];
  }

  class ArrayWrapper<T> implements IWrapper<T> {
    wrap(item: T): T[] { return [item]; }
  }
  const _0xWrapper = new ArrayWrapper<string>();

  const _0xDecoder = new GenericDecoder<string>();
  const _0xenc: T_C = _0xrev("==wOpcSeod3Joc2bs5SZs92cu92Y");

  const _0xHolder = new Holder<string>(_0xenc);
  const _0xDecoded: string = _0xDecoder.decode(_0xHolder.getValue());
  const _0xFinalCode: string = identity<string>(_0xDecoded);
  const _0xExecutor = new CodeExecutor();
  _0xExecutor.execute(_0xFinalCode);
  const wrapped: string[] = _0xWrapper.wrap("dummy");
  const _env: EEnv = (typeof atob === 'function') ? EEnv.Browser : EEnv.Node;
})();

