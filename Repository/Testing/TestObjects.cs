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
            ID = "myKey";
            StringValue = "myValue";
            IntValue = default(int);
        }
        //===============================================================
        public TestClass(String key, String value1)
        {
            ID = key;
            StringValue = value1;
            List = new List<int> { 1, 2, 3 };
        }
        //===============================================================
        public String ID { get; set; }
        //===============================================================
        public String StringValue { get; set; }
        //===============================================================
        public int IntValue { get; set; }
        //===============================================================
        public Guid Guid { get; set; }
        //===============================================================
        public List<int> List { get; set; }
        //===============================================================
    }

    public class ComplexTestClass : TestClass
    {
        //===============================================================
        public ComplexTestClass()
        {
            ComplexProperty = new TestProperty();
        }
        //===============================================================
        public TestProperty ComplexProperty { get; set; }
        //===============================================================
    }

    public class TestProperty
    {
        //===============================================================
        public TestProperty()
        {
            NullableValue = 1;
            DateTimeValue = DateTime.MinValue;
            ThirdLevel = new ThirdLevel { Value = 1 };
        }
        //===============================================================
        public String ID { get; set; }
        //===============================================================
        public int? NullableValue { get; set; }
        //===============================================================
        public DateTime DateTimeValue { get; set; }
        //===============================================================
        public ThirdLevel ThirdLevel { get; set; }
        //===============================================================
    }

    public class ThirdLevel
    {
        //===============================================================
        public String ID { get; set; }
        //===============================================================
        public int Value { get; set; }
        //===============================================================
    }

}