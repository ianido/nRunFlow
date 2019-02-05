using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nRunFlow;

namespace nRunFlow.Test
{
    [TestClass]
    public class FlowTests
    {
        int countGood = 0;
        int countBad = 0;
        void GoodStep(FlowStep step)
        {
            countGood++;
        }

        void BadStep(FlowStep step)
        {
            countBad++;
            throw new ApplicationException("Bad things happen");
        }

        [TestMethod]
        public void TestRealFlow()
        {
            var eng = new FlowEngine();
            countGood = 0; countBad = 0;

            eng.Start(a => GoodStep(a))
                .ContinueWith(a => GoodStep(a))
                .ContinueWith(a => GoodStep(a))
                .ContinueWith(a =>
                {
                    GoodStep(a);
                    eng.Start(b => GoodStep(b))
                        .IfSuccess(b => GoodStep(b))
                        .ContinueWith(b => GoodStep(b));

                    eng.Start(b => GoodStep(b))
                        .ContinueWith(b => GoodStep(b));

                    eng.Start(b => GoodStep(b))
                        .IfSuccess(b => GoodStep(b));
                })
                .ContinueWith(a =>
                {
                    GoodStep(a);
                    eng.Start(b => GoodStep(b))
                        .ContinueWith(b => GoodStep(b))
                        .ContinueWith(b => GoodStep(b))
                        .ContinueWith(b => GoodStep(b))
                        .ContinueWith(b => GoodStep(b));

                    eng.Start(b => GoodStep(b))
                        .IfSuccess(b => GoodStep(b))
                        .ContinueWith(b => GoodStep(b))
                        .ContinueWith(b => GoodStep(b));

                    eng.Start(b => GoodStep(b))
                        .IfSuccess(b => GoodStep(b))
                        .ContinueWith(b => GoodStep(b))
                        .ContinueWith(b => GoodStep(b));
                });

            Assert.AreEqual(countGood, eng.Steps.Count);
            Assert.IsTrue(eng.AllWasGood());
        }

        [TestMethod]
        public void TestSimpleFlow()
        {
            var eng = new FlowEngine();
            countGood = 0; countBad = 0;

            eng.Start(a => GoodStep(a))
                .ContinueWith(a => GoodStep(a))
                .ContinueWith(a => GoodStep(a));

            Assert.AreEqual(countGood, eng.Steps.Count);
            Assert.IsTrue(eng.AllWasGood());
        }

        [TestMethod]
        public void TestCountBad()
        {
            var eng = new FlowEngine();
            countGood = 0; countBad = 0;

            eng.Start(a => GoodStep(a))
                .ContinueWith(a => BadStep(a))
                .ContinueWith(a => GoodStep(a));

            Assert.AreEqual(countGood + countBad, eng.Steps.Count);
            Assert.AreEqual(countGood, eng.Steps.Count - countBad);
            Assert.AreEqual(countBad, eng.Steps.Count - countGood);

            Assert.IsFalse(eng.AllWasGood());
            Assert.IsTrue(eng.SomethingWasWrong());
        }


        [TestMethod]
        public void TestSimpleCondition()
        {
            var eng = new FlowEngine();
            countGood = 0; countBad = 0;
            eng.Start(a => GoodStep(a))
                .ContinueWith(a => BadStep(a))
                .IfSuccess(a => GoodStep(a)) // This will never be executed
                .IfFailed(a => GoodStep(a)); // This will never be executed

            Assert.AreEqual(1, countGood);

            Assert.AreEqual(countGood + countBad, eng.Steps.Count);
            Assert.AreEqual(countGood, eng.Steps.Count - countBad);
            Assert.AreEqual(countBad, eng.Steps.Count - countGood);

            Assert.IsFalse(eng.AllWasGood());
            Assert.IsTrue(eng.SomethingWasWrong());
        }


        [TestMethod]
        public void TestSimpleFork()
        {

            var eng = new FlowEngine();
            countGood = 0; countBad = 0;

            eng.Start(a => GoodStep(a))
                .ContinueWith(a => BadStep(a))
                .Where(new TupleList<Func<FlowStep, bool>, Action<FlowStep>>
                {
                    { a => a.Result.ResultCode == FlowStepResultValues.Success, a => BadStep(a) },
                    { a => a.Result.ResultCode == FlowStepResultValues.Failed, a => GoodStep(a) }
                })
                .IfAllSuccess(b => GoodStep(b));

            Assert.AreEqual(3, countGood);
            Assert.AreEqual(1, countBad);

            Assert.AreEqual(countGood + countBad, eng.Steps.Count);
            Assert.AreEqual(countGood, eng.Steps.Count - countBad);
            Assert.AreEqual(countBad, eng.Steps.Count - countGood);

            Assert.IsFalse(eng.AllWasGood());
            Assert.IsTrue(eng.SomethingWasWrong());
        }

        [TestMethod]
        public void TestSubFlowAffectingResultOfMainFlow()
        {
            var eng = new FlowEngine();
            countGood = 0; countBad = 0;
            eng.Start((a) =>
            {
                GoodStep(a);
            })
                .IfSuccess((a) =>
                {
                    var seng = new FlowEngine();
                    GoodStep(a);
                    seng.Start((b) =>
                    {
                        BadStep(b);
                    })
                    .ContinueWith((b) =>
                    {
                        GoodStep(b);
                    })
                    .ContinueWith((b) =>
                    {
                        GoodStep(b);
                    });
                    if (!seng.AllWasGood())
                    {
                        a.Result.ResultCode = FlowStepResultValues.Failed; //I am affecting the result of the task
                    }
                })
                .IfSuccess((a) =>
                {
                    GoodStep(a);
                });

            Assert.IsFalse(eng.AllWasGood());
            Assert.AreEqual(countGood, 4);
            Assert.AreEqual(countBad, 1);

        }
    }
}
