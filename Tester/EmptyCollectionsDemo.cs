﻿using System.Collections.ObjectModel;
using Tester.Runtime;
using static Nemesis.Demos.Extensions;

namespace Net8.Framework;

[Order(109)]
internal class EmptyCollectionsDemo : IShowable
{
    class KafkaService
    {
        bool dataIsReady = false;

        public IReadOnlyList<string> FetchData()
        {
            if (!dataIsReady) return new List<string>();//??
            else return GetData();
        }

        private IReadOnlyList<string> GetData() => throw new NotImplementedException();
    }


    public void Show()
    {
        DecompileAsCSharp(Method.Of(Arrays));

        DecompileAsCSharp(Method.Of(Collections));

        DecompileAsCSharp(Method.Of(CollectionExpressions));
    }

    private static void Arrays()
    {
        //0. enumerable
        var empty = Enumerable.Empty<string>();

        //1. arrays
        var emptyArray = new string[0]; //alloc
        //Span<string> onStack = stackalloc string[0]; //not possible + cannot return
        var betterEmpty = Array.Empty<string>();
    }

    private static void Collections()
    {
        var empty = ReadOnlyCollection<string>.Empty;
        var empty2 = ReadOnlyDictionary<string, int>.Empty;
        IDictionary<string, int> empty3 = ReadOnlyDictionary<string, int>.Empty;//possible but risky 
    }

    private static void CollectionExpressions()
    {
        Dictionary<string, int> dictionary = []; //alloc new every time 
        //ReadOnlyDictionary<string, int> pattern1 = []; //not supported
        //ReadOnlyCollection<string> list1 = [];//not supported
        IList<string> list = []; //new List<string>()
        IReadOnlyList<string> readOnlyList = []; //Array.Empty<string>()
        IReadOnlyCollection<string> readOnlyCollection = []; //Array.Empty<string>()        
        string[] array = []; //call to Array.Empty<string>();
    }
}