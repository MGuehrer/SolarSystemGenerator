using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Objects;

namespace Assets.Scripts.Collections
{
    /// <summary>
    /// TODO: Refactor this so instead of having a collection of ssobejcts, we have a tag on each object to say if it has been selected.
    /// </summary>
    public class SelectedObjects
    {
        public EventHandler CollectionModified;
        private readonly List<SSObject> _collection;

        public SelectedObjects()
        {
            _collection = new List<SSObject>();
        }

        public SelectedObjects(List<SSObject> collection)
        {
            _collection = collection;
        }

        public List<SSObject> GetAll()
        {
            return _collection;
        }

        public SSObject Get(SSObject child)
        {
            return _collection.FirstOrDefault(c => c.Equals(child));
        }

        public SSObject Get(int identifier)
        {
            return _collection.FirstOrDefault(c => c.Identifier.Equals(identifier));
        }

        public SSObject FirstOrDefault()
        {
            return _collection.FirstOrDefault();
        }

        public void Add(SSObject obj)
        {
            lock (_collection)
            {
                obj.IsSelected = true;
                _collection.Add(obj);
            }
            CollectionModified?.Invoke(this, null);
        }

        public void Remove(SSObject obj)
        {
            lock (_collection)
            {
                obj.IsSelected = false;
                _collection.Remove(obj);
            }
            CollectionModified?.Invoke(this, null);
        }

        public void Clear()
        {
            lock (_collection)
            {
                foreach (var child in _collection)
                {
                    child.IsSelected = false;
                }
                _collection.Clear();
            }
            CollectionModified?.Invoke(this, null);
        }

        public bool Contains(SSObject obj)
        {
            return _collection.Contains(obj);
        }

        public int Count()
        {
            return _collection.Count;
        }
    }
}
