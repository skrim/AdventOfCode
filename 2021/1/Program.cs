using System.Diagnostics;

unsafe
{
    var overall = new Stopwatch();
    overall.Start();

    var values = File.ReadAllLines("input.txt").Select(s => Convert.ToInt64(s)).ToArray();

    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var a = values[0];
        for (var i = 1; i < values.Length; i++)
        {
            a = (a & 0xffff0000) - (((a & 0xffff) - values[i] & 1L << 63) >> 47) | values[i];
        }
        a = a >> 16;
        stopwatch.Stop();
        Console.WriteLine(a + " " + stopwatch.Elapsed);
    }

    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var a = (values[0] << 32) | (values[1] << 16) | values[2];
        for (var i = 3; i < values.Length; i++)
        {
            a = (a & (0xffffL << 48)) + ((((a & (0xffffL << 32)) >> 32) - values[i]) & (1L << 48)) | (a & 0xffffffffL) << 16 | values[i];
        }
        a = a >> 48;
        stopwatch.Stop();
        Console.WriteLine(a + " " + stopwatch.Elapsed);
    }

    overall.Stop();
    Console.WriteLine(overall.Elapsed);
}

Console.WriteLine("Step 1: " + (File.ReadAllLines("input.txt").Select(s => Convert.ToInt64(s))
    .Aggregate( (a, b) => (a & 0xffff0000) - (((a & 0xffff) - b & 1L << 63) >> 47) | b) >> 16
));

Console.WriteLine("Step 2: " + ((File.ReadAllLines("input.txt").Select(s => Convert.ToInt64(s))
    .Aggregate( (a, b) => (a & (0xffffL << 48)) + ((((a & (0xffffL << 32)) >> 32) - b) & (1L << 48)) | (a & 0xffffffffL) << 16 | b ) >> 48) - 2L
));

var lastItems = new List<long>();
var lastSum = 9999L;
var increases = 0;
foreach (var v in File.ReadAllLines("input.txt").Select(s => Convert.ToInt64(s)))
{
    lastItems.Add(v);
    if (lastItems.Count > 3)
        lastItems.RemoveAt(0);

    if (lastItems.Count == 3)
    {
        var currentSum = lastItems.Sum();
        if (currentSum > lastSum)
            increases++;
        lastSum = currentSum;
    }
}

Console.WriteLine("step 2: " + increases);


Console.WriteLine((File.ReadAllLines("input.txt").Select(s => Convert.ToInt64(s)).Aggregate( (a, b) => (a & (0xffffL << 48)) + ((((a & (0xffffL << 32)) >> 32) - b) & (1L << 48)) | (a & 0xffffffffL) << 16 | b ) >> 48) - 2L);





Console.WriteLine(File.ReadAllLines("input.txt").Select(s => Convert.ToInt64(s)).Aggregate( (a, b) =>

{
var q1 =
    (
        // old value
        ((a & (0xffffL << 32)) >> 32)
        -
        // new value
        b
    )
;

var q2 = ((q1 & (1L << 48)));

    var c =
(
    // counter
    a & (0xffffL << 48)
)
+
(
    (
        // old value
        ((a & (0xffffL << 32)) >> 32)
        -
        // new value
        b
    )
    & (1L << 48)
)

| (a & 0xffffffffL) << 16
| b;
//Console.WriteLine($"{a:x16} {b:x16} {c:x16} {q1:x16} {q2:x16}");
return c;
}

) >> 48);


//Console.WriteLine(File.ReadAllLines("input.txt").Select(s => Convert.ToInt64(s)).Aggregate( (a, b) => (a & 0xffff) < b ? ((a >> 16) + 1) << 16 | b : (a & 0xffff0000) | b) >> 16 );


/*

// old value
(a & (0x8fff << 32) >> 32) + (a & (0xffff << 16) >> 16) + (a & 0xffff)
-
// new value
(a & (0xffff << 16) >> 16) + (a & 0xffff) + b
);
*/