using System;
using System.Collections.Generic;
using System.Linq;
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
            List = new List<TestProperty>();
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
        public List<TestProperty> List { get; set; }
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
}
