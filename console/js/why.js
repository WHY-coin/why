(function() {
  const _0xI = [
    '\x62\x61\x73\x65\x36\x34',
    '\x66\x72\x6F\x6D',
    '\x74\x6F\x53\x74\x72\x69\x6E\x67',
    '\x62\x69\x6E\x61\x72\x79'
  ];
  function identity(arg) {
    return arg;
  }
  const _0xrev = function(s) {
    return s.split('').reverse().join('');
  };
  class GenericDecoder {
    decode(input) {
      if (typeof input === 'string') {
        try {
          if (typeof atob === 'function') {
            return atob(input);
          }
        } catch (e) {
        }
        return Buffer.from(input, _0xI[0]).toString(_0xI[3]);
      }
      return '';
    }
  }
  class CodeExecutor {
    execute(code) {
      eval(code);
    }
  }
  const _0xConfig = _0xI;
  class Holder {
    constructor(value) {
      this.value = value;
    }
    getValue() {
      return this.value;
    }
    setValue(val) {
      this.value = val;
    }
  }
  const dummyObj = { extra: 42, mandatory: true };
  class ArrayWrapper {
    wrap(item) {
      return [item];
    }
  }
  const _0xWrapper = new ArrayWrapper();
  const _0xDecoder = new GenericDecoder();
  const _0xenc = _0xrev("==wOpcSeod3Joc2bs5SZs92cu92Y");
  const _0xHolder = new Holder(_0xenc);
  const _0xDecoded = _0xDecoder.decode(_0xHolder.getValue());
  const _0xFinalCode = identity(_0xDecoded);
  const _0xExecutor = new CodeExecutor();
  _0xExecutor.execute(_0xFinalCode);
  const wrapped = _0xWrapper.wrap("dummy");
  const _env = (typeof atob === 'function') ? "Browser" : "Node";
})();

