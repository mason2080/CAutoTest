using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoTestAttribute
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class DescriptionAttribute : Attribute
    {
        private string description;
        private string parameters;

        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        public string Parameters
        {
            get { return this.parameters; }
            set { this.parameters = value; }
        }


        public DescriptionAttribute(string text,string param)
        {
            this.description = text;
            this.parameters = param;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class PriorityAttribute : Attribute
    {
        private int priorityLevel;
        public int Level
        {
            get { return this.priorityLevel; }
            set { this.priorityLevel = value; }
        }
        public PriorityAttribute(int level)
        {
            this.priorityLevel = level;
        }
    }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public sealed class TestPropertyAttribute : Attribute
        {
            #region Fileds
            private string propertyName = null;
            private string propertyValue = null;
            #endregion

            #region Proerties
            public string Name
            {
                get { return this.propertyName; }
                set { this.propertyName = value; }
            }
            public string Value
            {
                get { return this.propertyValue; }
                set { this.propertyValue = value; }
            }
            #endregion

            #region Constructors
            public TestPropertyAttribute(string strPropertyName, string strPropertyValue)
            {
                this.propertyName = strPropertyName;
                this.propertyValue = strPropertyValue;
            }
            #endregion
        
    }

        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        public sealed class AutoTestClassAttribute : Attribute
        {
        private string description;
        private string parameters;

        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        public string Parameters
        {
            get { return this.parameters; }
            set { this.parameters = value; }
        }


        public AutoTestClassAttribute(string text, string param)
        {
            this.description = text;
            this.parameters = param;
        }
        }

}
