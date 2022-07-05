using System.Diagnostics;
using System.Runtime.CompilerServices;

const int Rounds = 1000000000;
const int Threads = 16;
const int Prisoners = 100;
const int BoxesAllowed = 50;

var totalWins = 0;
var signal = new ManualResetEventSlim(false);

void ProcessSingleThread()
{
    // Keep own copy of the array
    var boxes = Enumerable.Range(1, Prisoners).ToArray();
    var wins = 0;

    signal.Wait();

    for (var index = 0; index < Rounds / Threads; index++)
    {
        Shuffle(boxes);

        if (RunRound(boxes))
        {
            wins++;
        }
    }

    Interlocked.Add(ref totalWins, wins);
}

static void Shuffle(Span<int> array)
{
    var length = array.Length;

    while (length > 1)
    {
        var target = Random.Shared.Next(length--);
        (array[target], array[length]) = (array[length], array[target]);
    }
}

[MethodImpl(MethodImplOptions.AggressiveOptimization)]
static bool RunRound(Span<int> boxes)
{
    for (var index = 1; index <= Prisoners; index++)
    {
        var length = 0;
        var box = index;

        while (true)
        {
            box = boxes[box - 1];
            length++;

            if (length > BoxesAllowed)
            {
                return false;
            }

            if (box == index)
            {
                break;
            }
        }
    }

    return true;
}

var threads = new Thread[Threads];

for (var index = 0; index < Threads; index++)
{
    threads[index] = new Thread(ProcessSingleThread);
    threads[index].Start();
}

var stopwatch = Stopwatch.StartNew();
signal.Set();

for (var index = 0; index < Threads; index++)
{
    threads[index].Join();
}

stopwatch.Stop();

var result = totalWins / (double)Rounds;
Console.WriteLine(result);
Console.WriteLine($"Took: {stopwatch.Elapsed}");
