using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Repository.Tests
{
    class TestClass
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
        public List<int> IntegerList { get; set; }
        //===============================================================
        public List<TestProperty> ComplexList { get; set; }
        //===============================================================
        public Guid Guid { get; set; }
        //===============================================================
    }

    class TestProperty
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
    }

    class ThirdLevel
    {
        public int Value { get; set; }
    }

    /// <summary>
    /// Describes the status of a work order.
    /// </summary>
    [DataContract]
    public enum WorkOrderStatus
    {
        //===============================================================
        /// <summary>
        /// Indicates that a work order has not yet been started.
        /// </summary>
        [EnumMember]
        NotStarted,
        //===============================================================
        /// <summary>
        /// Indicates that a work order has been started but not yet completed.
        /// </summary>
        [EnumMember]
        Started,
        //===============================================================
        /// <summary>
        /// Indicates that a work order has been completed.
        /// </summary>
        [EnumMember]
        Complete
        //===============================================================
    }

    /// <summary>
    /// A work order, i.e. a maintenance request for a specific unit.
    /// </summary>
    [DataContract]
    public class WorkOrder
    {
        //===============================================================
        /// <summary>
        /// The unique ID of this work order.
        /// </summary>
        [DataMember]
        public Guid ID { get; set; }
        //===============================================================
        /// <summary>
        /// The data for this work order.
        /// </summary>
        [DataMember]
        public WorkOrderData Data { get; set; }
        //===============================================================
    }

    /// <summary>
    /// A helper class that holds client-specified data for work orders.
    /// </summary>
    [DataContract]
    public class WorkOrderData
    {
        //===============================================================
        /// <summary>
        /// The ID of the technician assigned to this job.
        /// </summary>
        [DataMember]
        public String TechnicianID { get; set; }
        //===============================================================
        /// <summary>
        /// The ID of the unit that this work order applies to.
        /// </summary>
        [DataMember]
        public Guid UnitID { get; set; }
        //===============================================================
        /// <summary>
        /// The name of this work order.
        /// </summary>
        [DataMember]
        public String Name { get; set; }
        //===============================================================
        /// <summary>
        /// The statement associated with this work order.
        /// </summary>
        [DataMember]
        public String Statement { get; set; }
        //===============================================================
        /// <summary>
        /// The status of this work order.
        /// </summary>
        [DataMember]
        public WorkOrderStatus Status { get; set; }
        //===============================================================
        /// <summary>
        /// The comment of this work order.
        /// </summary>
        [DataMember]
        public String Comment { get; set; }
        //===============================================================
        /// <summary>
        /// A list of the IDs of any media associated with this work order.
        /// </summary>
        [DataMember]
        public List<Guid> MediaIDs { get; set; }
        //===============================================================
    }

    /// <summary>
    /// A fixture is a landlord-owned item in that exists inside a unit, such as a couch or a lamp.
    /// </summary>
    [DataContract]
    public class Fixture
    {
        //===============================================================
        /// <summary>
        /// The ID of this fixture.
        /// </summary>
        [DataMember]
        public Guid FixtureID { get; set; }
        //===============================================================
        /// <summary>
        /// The data for this fixture.
        /// </summary>
        [DataMember]
        public FixtureData Data { get; set; }
        //===============================================================
    }

    /// <summary>
    /// Describes the Condition of a fixture, i.e. whether it is new, worn, broken, etc.
    /// </summary>
    [DataContract]
    public enum FixtureCondition
    {
        //===============================================================
        /// <summary>
        /// New fixture, i.e. no marks/scratches.
        /// </summary>
        [EnumMember]
        New,
        //===============================================================
        /// <summary>
        /// Worn, i.e. scratched or scuffed.
        /// </summary>
        [EnumMember]
        Worn,
        //===============================================================
        /// <summary>
        /// Broken, i.e. unusable.
        /// </summary>
        [EnumMember]
        Broken,
        //===============================================================
        /// <summary>
        /// Missing, i.e. lost.
        /// </summary>
        [EnumMember]
        Missing,
        //===============================================================
    }

    /// <summary>
    /// Contains data about a specific fixture.
    /// </summary>
    [DataContract]
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
        [DataMember]
        public String Name { get; set; }
        //===============================================================
        /// <summary>
        /// The Condition of this fixture, i.e. Broken.
        /// </summary>
        [DataMember]
        public FixtureCondition Condition { get; set; }
        //===============================================================
        /// <summary>
        /// The ID unit that contains this fixture (null if it is currently in inventory).
        /// </summary>
        [DataMember]
        public Guid? UnitID { get; set; }
        //===============================================================
        /// <summary>
        /// A list of comments and their associated dates. An example might be:
        /// { "Scuffed by tenant", 12/9/2011 }
        /// </summary>
        [DataMember]
        public List<DateValuePair<String>> Comments { get; set; }
        //===============================================================
        /// <summary>
        /// A list of the IDs of all media associated with this fixture.
        /// </summary>
        [DataMember]
        public List<Guid> MediaIDs { get; set; }
        //===============================================================
    }

    /// <summary>
    /// A helper class that contains a date/value pair.
    /// </summary>
    [DataContract]
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
        [DataMember]
        public DateTime Date { get; set; }
        //===============================================================
        [DataMember]
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
