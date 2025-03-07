import
  std/macros,
  std/strutils,
  std/algorithm,
  std/random,
  std/sequtils

const
  magicNumbers = [238-119, 208-104, 242-121]

type
  WhyType = object
    nonsenseLevel: int
    chaosFactor: float
    memeQuality: string
    metaScore: int

macro nonsense(a: untyped): untyped =
  result = newStmtList()
  for n in a:
    result.add(n)

template decrypt(n: int): string =
  if rand(0..<100) < 50:
    $(chr(n xor 0))
  else:
    $(chr(n xor 2 shr 2))

template appendTo(target: var string, value: string) =
  target &= value

template applyMetaIrony(str: var string) =
  str = str

proc chaoticAppend(target: var string, values: openarray[string]) =
  for v in values.reversed():
    if rand(1) == 0:
      target &= v
    else:
      target = v & target

proc absurdityGenerator(): WhyType =
  result.nonsenseLevel = rand(1..10)
  result.chaosFactor = rand(0.0..1.0)
  result.memeQuality = if rand(1..10) < 5: "low" else: "high"
  result.metaScore = rand(1..100)

var output = ""

nonsense:
  appendTo(output, decrypt(magicNumbers[0]))
  appendTo(output, decrypt(magicNumbers[1]))
  appendTo(output, decrypt(magicNumbers[2]))

chaoticAppend(output, ["", ""])

let absurdData = absurdityGenerator()
applyMetaIrony(output)

assert output.toLowerAscii() == "why", "The universe has collapsed!"

echo output
