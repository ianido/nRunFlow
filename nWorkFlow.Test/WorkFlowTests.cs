using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflow;

namespace nWorkFlow.Test
{
    [TestClass]
    public class WorkFlowTests
    {
        int countGood = 0;
        int countBad = 0;
        void GoodStep(WorkFlowStep step)
        {
            countGood++;
        }

        void BadStep(WorkFlowStep step)
        {
            countBad++;
            throw new ApplicationException("Bad things happen");
        }

        [TestMethod]
        public void TestRealFlow()
        {
            var eng = new WorkFlowEngine();
            countGood = 0; countBad = 0;
            eng.Start((a) =>
            {
                GoodStep(a);
            })
                .ContinueWith((a) =>
                {
                    GoodStep(a);
                })
                .ContinueWith((a) =>
                {
                    GoodStep(a);
                })
                .ContinueWith((a) =>
                {
                    GoodStep(a);
                    eng.Start((b) =>
                    {
                        GoodStep(b);
                    })
                    .IfSuccess((b) =>
                    {
                        GoodStep(b);
                    })
                    .ContinueWith((b) =>
                    {
                        GoodStep(b);
                    });

                    eng.Start((b) =>
                    {
                        GoodStep(b);
                    })
                    .ContinueWith((b) =>
                    {
                        GoodStep(b);
                    });

                    eng.Start((b) =>
                    {
                        GoodStep(b);
                    })
                    .IfSuccess((b) =>
                    {
                        GoodStep(b);
                    });
                })
                .ContinueWith((a) =>
                {
                    GoodStep(a);
                    eng.Start((b) =>
                    {
                        GoodStep(b);
                    })
                    .ContinueWith((b) =>
                    {
                        GoodStep(b);
                    })
                    .ContinueWith((b) =>
                    {
                        GoodStep(b);
                    })
                    .ContinueWith((b) =>
                    {
                        GoodStep(b);
                    })
                    .ContinueWith((b) =>
                    {
                        GoodStep(b);
                    });

                    eng.Start((b) =>
                    {
                        GoodStep(b);
                    })
                    .IfSuccess((b) =>
                    {
                        GoodStep(b);
                    })
                    .ContinueWith((b) =>
                    {
                        GoodStep(b);
                    })
                    .ContinueWith((b) =>
                    {
                        GoodStep(b);
                    });

                    eng.Start((b) =>
                    {
                        GoodStep(b);
                    })
                        .IfSuccess((b) =>
                        {
                            GoodStep(b);
                        })
                        .ContinueWith((b) =>
                        {
                            GoodStep(b);
                        })
                        .ContinueWith((b) =>
                        {
                            GoodStep(b);
                        });
                });

            Assert.AreEqual(countGood, eng.Steps.Count);
            Assert.IsTrue(eng.AllWasGood());
        }

        [TestMethod]
        public void TestSimpleFlow()
        {
            var eng = new WorkFlowEngine();
            countGood = 0; countBad = 0;
            eng.Start((a) =>
            {
                GoodStep(a);
            })
            .ContinueWith((a) =>
            {
                GoodStep(a);
            })
            .ContinueWith((a) =>
            {
                GoodStep(a);
            });


            Assert.AreEqual(countGood, eng.Steps.Count);
            Assert.IsTrue(eng.AllWasGood());

        }

        [TestMethod]
        public void TestCountBad()
        {
            var eng = new WorkFlowEngine();
            countGood = 0; countBad = 0;
            eng.Start((a) =>
            {
                GoodStep(a);
            })
            .ContinueWith((a) =>
            {
                BadStep(a);
            })
            .ContinueWith((a) =>
            {
                GoodStep(a);
            });


            Assert.AreEqual(countGood + countBad, eng.Steps.Count);
            Assert.AreEqual(countGood, eng.Steps.Count - countBad);
            Assert.AreEqual(countBad, eng.Steps.Count - countGood);

            Assert.IsFalse(eng.AllWasGood());
            Assert.IsTrue(eng.SomethingWasWrong());
        }


        [TestMethod]
        public void TestSimpleCondition()
        {
            var eng = new WorkFlowEngine();
            countGood = 0; countBad = 0;
            eng.Start((a) =>
            {
                GoodStep(a);
            })
            .ContinueWith((a) =>
            {
                BadStep(a);
            })
            .IfSuccess((a) =>
            {
                // This will never be executed
                GoodStep(a);
            })
            .IfFailed((a) =>
            {
                // This will never be executed
                GoodStep(a);
            });

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

            var eng = new WorkFlowEngine();
            countGood = 0; countBad = 0;
            eng.Start((a) =>
            {
                GoodStep(a);
            })
            .ContinueWith((a) =>
            {
                BadStep(a);
            })
            .Where(new TupleList<Func<WorkFlowStep, bool>, Action<WorkFlowStep>>{
                { (a) => a.Result.ResultCode == WorkFlowStepResultValues.Success, (a) => {
                    BadStep(a); }},
                { (a) => a.Result.ResultCode == WorkFlowStepResultValues.Failed, (a) => {
                    GoodStep(a); }}
            }).IfAllSuccess((b) =>
            {
                GoodStep(b);
            });

            Assert.AreEqual(3, countGood);
            Assert.AreEqual(1, countBad);

            Assert.AreEqual(countGood + countBad, eng.Steps.Count);
            Assert.AreEqual(countGood, eng.Steps.Count - countBad);
            Assert.AreEqual(countBad, eng.Steps.Count - countGood);

            Assert.IsFalse(eng.AllWasGood());
            Assert.IsTrue(eng.SomethingWasWrong());
        }
    }
}
