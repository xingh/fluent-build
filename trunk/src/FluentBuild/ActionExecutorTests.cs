﻿using NUnit.Framework;

namespace FluentBuild
{
    [TestFixture]
    public class ActionExecutorTests
    {
        [Test]
        public void ShouldApplyValuesAndExecute()
        {
            //verifies that actions are applied
            //the results are moved to a static variable of OutputValue if sucessfull
            InternalExecutableTester.OutputValue = "";
            var subject = new ActionExcecutor();
            var value = "aw4sagh34";
            subject.Execute<InternalExecutableTester>(x=>x.SetTestValue(value));
            Assert.That(InternalExecutableTester.OutputValue, Is.EqualTo(value));

        }

        [Test]
        public void ShouldApplyValuesToItemWithConstructorAndExecute()
        {
            //verifies that actions are applied
            //the results are moved to a static variable of OutputValue if sucessfull
            InternalExecutableTester.OutputValue = "";
            var subject = new ActionExcecutor();
            var value = "aw4sagh34";
            string constructorParms = "test";
            subject.Execute<InternalExecutableTester, string>(x => x.SetTestValue(value), constructorParms);
            Assert.That(InternalExecutableTester.OutputValue, Is.EqualTo(value));
            Assert.That(InternalExecutableTester.ConstructorValue, Is.EqualTo(constructorParms));

        }

        public class InternalExecutableTester : InternalExecuatable
        {
            public string TestValue { get; set; }
            public static string OutputValue { get; set; }
            public static string ConstructorValue { get; set; }

            public InternalExecutableTester()
            {
                
            }
            
            public InternalExecutableTester(string constructorValue)
            {
                ConstructorValue = constructorValue;
            }

            public void SetTestValue(string value)
            {
                TestValue = value;
            }

            internal override void InternalExecute()
            {
                //copy Test to Output to ensure the actions were applied
                OutputValue = TestValue;
            }
        }


    }
}