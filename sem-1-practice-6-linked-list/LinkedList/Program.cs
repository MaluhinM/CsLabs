namespace LinkedList;

class LinkedList
{
    private Element? _head;
    private Element? _cursor;

    private class Element(int value, int index, Element? next = null)
    {
        public int Value = value;
        public int Index = index;
        public Element? Next = next;
    }

    private void ResetCursor()
    {
        _cursor = _head;
    }

    private void InitHead(int value)
    {
        _head ??= new(value, 0);
    }

    public void InsertAt(int index, int value)
    {
        if (index == 0)
        {
            InsertAtBegin(value);
        }
        InitHead(value);
        ResetCursor();
        int currentIndex = 0;
        bool success = false;
        while (_cursor!.Next != null)
        {
            if (!success && currentIndex == _cursor.Index)
            {
                _cursor!.Next = new(value, _cursor.Index + 1, _cursor.Next);
                success = true;
            }
            if (success)
            _cursor = _cursor.Next;
            _cursor.Index++;
            currentIndex++;
        }
    }

    public void InsertAtBegin(int value)
    {
        Element elem = new(value, 0, _head);
        _head = elem;
    }

    public void InsertAtEnd(int value)
    {
        ResetCursor();
        while (_cursor!.Next != null)
        {
            _cursor = _cursor.Next;
        }
        _cursor.Next = new(value, _cursor.Index + 1);
    }
}

class Program
{
    public static void Main()
    {
        LinkedList linkedList = new();
        linkedList.InsertAtBegin(1);
        linkedList.InsertAtBegin(2);
        linkedList.InsertAtBegin(35);
    }
}
