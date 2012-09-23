using System;
using System.Collections.Generic;

namespace Repository
{
    public class StringKeyClass
    {
        //===============================================================
        public StringKeyClass(String key)
        {
            Key = key;
        }
        //===============================================================
        public String Key { get; set; }
        //===============================================================
    }

    public class TestClass
    {
        //===============================================================
        public TestClass()
        {
            Key = 1;
            Value1 = 1;
            Value2 = DateTime.MinValue;
            Property = new TestProperty();
            ComplexList = new List<TestProperty>();
            IntegerList = new List<int>();
        }
        //===============================================================
        public int Key { get; set; }
        //===============================================================
        public int Value1 { get; set; }
        //===============================================================
        public DateTime Value2 { get; set; }
        //===============================================================
        public TestProperty Property { get; set; }
        //===============================================================
        public String StringProperty { get; set; }
        //===============================================================
        public List<int> IntegerList { get; set; }
        //===============================================================
        public List<TestProperty> ComplexList { get; set; }
        //===============================================================
        public Guid Guid { get; set; }
        //===============================================================
    }

    public class TestProperty
    {
        //===============================================================
        public TestProperty()
        {
            Value1 = 1;
            Value2 = DateTime.MinValue;
            ThirdLevel = new ThirdLevel { Value = 1 };
        }
        //===============================================================
        public int? Value1 { get; set; }
        //===============================================================
        public DateTime Value2 { get; set; }
        //===============================================================
        public ThirdLevel ThirdLevel { get; set; }
        //===============================================================
    }

    public class ThirdLevel
    {
        public int Value { get; set; }
    }

    /// <summary>
    /// Describes the status of a work order.
    /// </summary>
    public enum WorkOrderStatus
    {
        //===============================================================
        /// <summary>
        /// Indicates that a work order has not yet been started.
        /// </summary>
        NotStarted,
        //===============================================================
        /// <summary>
        /// Indicates that a work order has been started but not yet completed.
        /// </summary>
        Started,
        //===============================================================
        /// <summary>
        /// Indicates that a work order has been completed.
        /// </summary>
        Complete
        //===============================================================
    }

    /// <summary>
    /// A work order, i.e. a maintenance request for a specific unit.
    /// </summary>
    public class WorkOrder
    {
        //===============================================================
        /// <summary>
        /// The unique ID of this work order.
        /// </summary>
        public Guid ID { get; set; }
        //===============================================================
        /// <summary>
        /// The data for this work order.
        /// </summary>
        public WorkOrderData Data { get; set; }
        //===============================================================
    }

    /// <summary>
    /// A helper class that holds client-specified data for work orders.
    /// </summary>
    public class WorkOrderData
    {
        //===============================================================
        /// <summary>
        /// The ID of the technician assigned to this job.
        /// </summary>
        public String TechnicianID { get; set; }
        //===============================================================
        /// <summary>
        /// The ID of the unit that this work order applies to.
        /// </summary>
        public Guid UnitID { get; set; }
        //===============================================================
        /// <summary>
        /// The name of this work order.
        /// </summary>
        public String Name { get; set; }
        //===============================================================
        /// <summary>
        /// The statement associated with this work order.
        /// </summary>
        public String Statement { get; set; }
        //===============================================================
        /// <summary>
        /// The status of this work order.
        /// </summary>
        public WorkOrderStatus Status { get; set; }
        //===============================================================
        /// <summary>
        /// The comment of this work order.
        /// </summary>
        public String Comment { get; set; }
        //===============================================================
        /// <summary>
        /// A list of the IDs of any media associated with this work order.
        /// </summary>
        public List<Guid> MediaIDs { get; set; }
        //===============================================================
    }

    /// <summary>
    /// A fixture is a landlord-owned item in that exists inside a unit, such as a couch or a lamp.
    /// </summary>
    public class Fixture
    {
        //===============================================================
        /// <summary>
        /// The ID of this fixture.
        /// </summary>
        public Guid FixtureID { get; set; }
        //===============================================================
        /// <summary>
        /// The data for this fixture.
        /// </summary>    
        public FixtureData Data { get; set; }
        //===============================================================
    }

    /// <summary>
    /// Describes the Condition of a fixture, i.e. whether it is new, worn, broken, etc.
    /// </summary>    
    public enum FixtureCondition
    {
        //===============================================================
        /// <summary>
        /// New fixture, i.e. no marks/scratches.
        /// </summary>
        New,
        //===============================================================
        /// <summary>
        /// Worn, i.e. scratched or scuffed.
        /// </summary>
        Worn,
        //===============================================================
        /// <summary>
        /// Broken, i.e. unusable.
        /// </summary>
        Broken,
        //===============================================================
        /// <summary>
        /// Missing, i.e. lost.
        /// </summary>
        Missing,
        //===============================================================
    }

    /// <summary>
    /// Contains data about a specific fixture.
    /// </summary>  
    public class FixtureData
    {
        //===============================================================
        public FixtureData()
        {
            Comments = new List<DateValuePair<string>>();
            MediaIDs = new List<Guid>();
        }
        //===============================================================
        /// <summary>
        /// The name of this fixture, i.e. "Living Room Couch".
        /// </summary>        
        public String Name { get; set; }
        //===============================================================
        /// <summary>
        /// The Condition of this fixture, i.e. Broken.
        /// </summary>        
        public FixtureCondition Condition { get; set; }
        //===============================================================
        /// <summary>
        /// The ID unit that contains this fixture (null if it is currently in inventory).
        /// </summary>        
        public Guid? UnitID { get; set; }
        //===============================================================
        /// <summary>
        /// A list of comments and their associated dates. An example might be:
        /// { "Scuffed by tenant", 12/9/2011 }
        /// </summary>        
        public List<DateValuePair<String>> Comments { get; set; }
        //===============================================================
        /// <summary>
        /// A list of the IDs of all media associated with this fixture.
        /// </summary>        
        public List<Guid> MediaIDs { get; set; }
        //===============================================================
    }

    /// <summary>
    /// A helper class that contains a date/value pair.
    /// </summary>    
    public class DateValuePair<T>
    {
        //===============================================================
        public DateValuePair()
        {
            Date = default(DateTime);
            Value = default(T);
        }
        //===============================================================
        public DateValuePair(DateTime date, T value)
        {
            Date = date;
            Value = value;
        }
        //===============================================================        
        public DateTime Date { get; set; }
        //===============================================================       
        public T Value { get; set; }
        //===============================================================
    }

    public static class DateValuePair
    {
        //===============================================================
        public static DateValuePair<T> Create<T>(DateTime date, T value)
        {
            return new DateValuePair<T>(date, value);
        }
        //===============================================================
    }
}
