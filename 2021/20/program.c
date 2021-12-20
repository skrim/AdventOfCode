#include <stdio.h>
#include <stdbool.h>
#include <string.h>

#define MAPPING 512
#define CANVAS 300
#define ITERATIONS 50
#define read(x, y, p) ((x >= 0 && x < CANVAS && y >= 0 && y < CANVAS && image[x][y][i % 2]) << p)
#define forYX for (int y = 0; y < CANVAS; y++) for (int x = 0; x < CANVAS; x++)

int main() {
    bool transform[MAPPING];
    bool image[CANVAS][CANVAS][2];

    forYX image[x][y][0] = false;

    FILE *fp = fopen("input.txt", "r");
    char buffer[MAPPING];
    int line = 0;
    while (fgets(buffer, MAPPING, fp)) {
        for (int i = 0; i < strlen(buffer) - 1; i++) {
            if (line == 0) transform[i] = buffer[i] == '#';
            if (line >= 2) image[i + ITERATIONS * 2][line - 2 + ITERATIONS * 2][0] = buffer[i] == '#';
        }
        line++;
    }
    fclose(fp);

    for (int i = 0; i < ITERATIONS; i++) {
        int count = 0;
        forYX {
            bool value = transform[ read(x - 1, y - 1, 8) | read(x, y - 1, 7) | read(x + 1, y - 1, 6) |
                                    read(x - 1, y    , 5) | read(x, y    , 4) | read(x + 1, y    , 3) |
                                    read(x - 1, y + 1, 2) | read(x, y + 1, 1) | read(x + 1, y + 1, 0)   ];
            image[x][y][(i + 1) % 2] = value;
            if (x >= ITERATIONS && y >= ITERATIONS && x <= CANVAS - ITERATIONS && y <= CANVAS - ITERATIONS && value) count++;
        }
        if (i == 1 || i == 49) printf("Part %d: %d\n", i == 1 ? 1 : 2, count);
    }
    return 0;
}