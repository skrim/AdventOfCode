use v6;

my $file = open 'input.txt';
my @world;

my $w = 0, my $h = 0;

my $y = 0;
for $file.lines -> $line {
    next unless $line;
    my $x = 0;
    for $line.comb -> $char {
        @world[$x; $y] = +$char;
        $x++;
    }
    $w = $x;
    $y++;
}
$h = $y;

my $totalrisk = 0, my @basinsizes;

for 0..$h - 1 -> $y {
    for 0..$w - 1 -> $x {
        my $p = @world[$x; $y];

        if ($x == 0 || @world[$x - 1; $y] > $p) &&
           ($x == $w - 1 || @world[$x + 1; $y] > $p) &&
           ($y == 0 || @world[$x; $y - 1] > $p) &&
           ($y == $h - 1 || @world[$x; $y + 1] > $p) {

            $totalrisk += $p + 1;

            my $basinsize = 0;
            my @stack;
            @stack.push: ($x, $y);
            repeat while @stack != () {
                my ($bx, $by) = @stack.pop;

                if @world[$bx; $by] < 9 {
                    $basinsize++;
                    @world[$bx; $by] = 9;

                    @stack.push: ($bx - 1, $by) if $bx > 0;
                    @stack.push: ($bx + 1, $by) if $bx < $w - 1;
                    @stack.push: ($bx, $by - 1) if $by > 0;
                    @stack.push: ($bx, $by + 1) if $by < $h - 1;
                }
            };

            @basinsizes.push: $basinsize;
        }
    }
}

say "Step 1: ", $totalrisk;
say "Step 2: ", @basinsizes.sort.reverse[0..2].reduce(&infix:<*>);
