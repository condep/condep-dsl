using System;
using System.Globalization;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Validation;
using NUnit.Framework;

namespace ConDep.Dsl.Tests
{
    [TestFixture]
    public class RemoteServerOperationTests
    {
        [Test]
        public void TestThat_ValidationOfMultipleConstructorsSucceeds()
        {
            var op = new MyRemoteOpWithMultipleConstructors("asdf", new ComplexObj());
            var notification = new Notification();
            Assert.That(op.IsValid(notification), Is.True);
            Assert.That(notification.HasErrors, Is.False);
        }

        [Test]
        public void TestThat_ValidationOfEmptyConstructorSecceeds()
        {
            var op = new MyRemoteOpWithMultipleConstructors();
            var notification = new Notification();
            Assert.That(op.IsValid(notification), Is.True);
            Assert.That(notification.HasErrors, Is.False);
        }

        [Test]
        public void TestThat_ValidationFailsIfOrderOfConstructorArgumentsIsWrong()
        {
            var op = new MyRemoteOpWithWrongConstructorOrder("hello!", new ComplexObj());
            var notification = new Notification();
            Assert.That(op.IsValid(notification), Is.False);
            Assert.That(notification.HasErrors, Is.True);
        }
    }

    public class MyRemoteOpWithSingleConstructor : RemoteCodeOperation
    {
        private readonly string _someMessage;
        private readonly int _someVal;
        private readonly ComplexObj _complex;

        public MyRemoteOpWithSingleConstructor(string someMessage, int someVal, ComplexObj complex) : base(someMessage, someVal, complex)
        {
            _someMessage = someMessage;
            _someVal = someVal;
            _complex = complex;
            _complex.Value = "Complex value";
        }

        public override void Execute(ILogForConDep logger)
        {
            logger.Info(_someMessage);
            logger.Info(_someVal.ToString(CultureInfo.InvariantCulture));
            logger.Info(_complex.Value);
        }

        public override string Name
        {
            get { throw new System.NotImplementedException(); }
        }
    }

    public class ComplexObj
    {
        public string Value { get; set; }
    }

    public class MyRemoteOpWithWrongConstructorOrder : RemoteCodeOperation
    {
        public MyRemoteOpWithWrongConstructorOrder(string val, ComplexObj complx)
            : base(complx, val)
        {

        }
        public override void Execute(ILogForConDep logger)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class MyRemoteOpWithMultipleConstructors : RemoteCodeOperation
    {
        public MyRemoteOpWithMultipleConstructors()
        {

        }

        public MyRemoteOpWithMultipleConstructors(string tmp) : base(tmp)
        {
        }

        public MyRemoteOpWithMultipleConstructors(string tmp, ComplexObj complex) : base(tmp, complex)
        {
        }

        public override void Execute(ILogForConDep logger)
        {
            throw new System.NotImplementedException();
        }

        public override string Name
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}