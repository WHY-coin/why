-module(hello).
-export([why/0]).
why() -> io:fwrite("Why?\n").
