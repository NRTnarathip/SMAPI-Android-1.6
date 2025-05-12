// See https://aka.ms/new-console-template for more information
using System.Collections.ObjectModel;
using System.Collections.Specialized;

Console.WriteLine("Hello, World!");


var players = new List<string>();
var playersWatcher = new ObservableCollection<string>(players);
playersWatcher.CollectionChanged += OnCollectionChanged;

for (int i = 0; i < 10; i++)
{
    players.Add("hello: " + i);
}

static void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
{
    Console.WriteLine($"OnCollectionChanged:{notifyCollectionChangedEventArgs.Action}");
}

Console.WriteLine("done");
Console.WriteLine(playersWatcher.Count);
