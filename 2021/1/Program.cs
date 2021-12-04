var values = File.ReadAllLines("input.txt").Select(s => Convert.ToInt64(s)).ToArray();

var result1 = values.Aggregate( (a, b) => (a & 0xffff0000) - (((a & 0xffff) - b & 1L << 63) >> 47) | b) >> 16;
Console.WriteLine($"Step 1: {result1}");

var result2 = (values.Aggregate( (a, b) => (a & (0xffffL << 48)) + ((((a & (0xffffL << 32)) >> 32) - b) & (1L << 48)) | (a & 0xffffffffL) << 16 | b ) >> 48) - 2L;
Console.WriteLine($"Step 2: {result2}");