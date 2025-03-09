#include <stdio.h>

int main() {
  for (int i = 0; i < 3; i++)
    printf("%c", 0b1100000 | ((i & 1) ? 0 : 0b10001) | ((i > 0) << 3) |
                     (((i == 0) << 1) * 3));
  printf("\n");
}
