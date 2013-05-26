using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KellermanSoftware.CompareNetObjects;

namespace Repository.ChangeTracking
{
    internal class ChangeTracker<T>
    {
        //===============================================================
        static ChangeTracker()
        {
            CanTrackChanges = typeof(T).GetConstructor(Type.EmptyTypes) != null;
        }
        //===============================================================
        public ChangeTracker(T value, Func<T, object[]> keySelector)
        {
            CurrentValue = value;
            KeySelector = keySelector;
            InitialValue = CanTrackChanges ? (T)Activator.CreateInstance(typeof(T)) : default(T);
            if (CanTrackChanges)
                AutoMapper.Mapper.DynamicMap(CurrentValue, InitialValue);
        }
        //===============================================================
        public static bool CanTrackChanges { get; private set; }
        //===============================================================
        public Func<T, Object[]> KeySelector { get; private set; }
        //===============================================================
        public T CurrentValue { get; private set; }
        //===============================================================
        public T InitialValue { get; private set; }
        //===============================================================
        internal bool HasChanges
        {
            get
            {
                if (!KeySelector(CurrentValue).SequenceEqual(KeySelector(InitialValue)))
                    throw new InvalidOperationException("You cannot change an object's key value.");

                return CanTrackChanges && !new CompareObjects().Compare(CurrentValue, InitialValue);
            }
        }
        //===============================================================
    }
}
