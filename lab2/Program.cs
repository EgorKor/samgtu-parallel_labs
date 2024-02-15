// See https://aka.ms/new-console-template for more information
using System.Collections;

Console.WriteLine("Hello, World!");
//recursive_go(new List<int>() { 1,2,3,4,5},5);
//recursive_go_1(2, 5);
List<string> sequence = getSequenceRecursive(4,4);
foreach(string s in sequence)
{
    Console.WriteLine(s);
}



List<string> getSequenceRecursive(int m, int n)
{
    List<string> sequenceList = new List<string>();
    for(int i = 0; i <= n; i++)
    {
        recursiveSequence(m - 1, n, "" + i, i, sequenceList);
    }
    return sequenceList;
}

void recursiveSequence(int m, int n, string last, int last_int, List<string> sequenceList)
{
    if(m == 0)
    {
        sequenceList.Add(last);
        return;
    }
    for (int i = 0; i <= n; i++)
    {
        if (last_int != i)
        {   
            recursiveSequence(m - 1, n, last + i, i, sequenceList);
        }
    }
    

}