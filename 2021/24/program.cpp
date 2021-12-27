#include <fstream>
#include <iostream>
#include <sstream>
#include <string>
#include <cstddef>

class Solver {
    long as[14], bs[14], cs[14];

    long next(long input, long z, int step) {
        long v = input % 10;
        long x = (z % 26 + bs[step]) == v ? 0 : 1;
        return z / as[step] * (25 * x + 1) + (v + cs[step]) * x;
    }

    long getModulo(long z, int step) {
        return z % 26 + bs[step];
    }

    public:
        Solver() {}

        void initA(int index, long value) {
            as[index] = value;
        }

        void initB(int index, long value) {
            bs[index] = value;
        }

        void initC(int index, long value) {
            cs[index] = value;
        }

        long minSolution = 99999999999999;
        long maxSolution = 0;

        void solve(long input, long z, int step) {
            if (step == 14) {
                if (z == 0) {
                    if (input < minSolution) minSolution = input;
                    if (input > maxSolution) maxSolution = input;
                }
                return;
            }

            if (as[step] == 26) {
                long m = getModulo(z, step);
                if (m < 1 || m > 9) return;
                long n = input * 10 + m;
                z = next(n, z, step);
                solve(n, z, step + 1);
            } else {
                for (long digit = 1; digit < 10; digit++) {
                    long n = input * 10 + digit;
                    long z2 = next(n, z, step);
                    solve(n, z2, step + 1);
                }
            }

        }
};

int main() {
    Solver s;

    std::ifstream input("input.txt");
    int c = 0;
    for( std::string line; getline( input, line ); ) {
        std::size_t pos = line.find_last_of(" ");
        std::string last = line.substr(pos + 1);
        if (c % 18 == 4) s.initA(c / 18, stoi(last));
        if (c % 18 == 5) s.initB(c / 18, stoi(last));
        if (c % 18 == 15) s.initC(c / 18, stoi(last));
        c++;
    }

    s.solve(0, 0, 0);
    std::cout << "Part 1: " + std::to_string(s.maxSolution) + "\n";
    std::cout << "Part 2: " + std::to_string(s.minSolution) + "\n";
    return 0;
}