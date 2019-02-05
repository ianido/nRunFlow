using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace nRunFlow
{
    public enum FlowStepResultValues
    {
        [Description("NotExecuted")]
        NotExecuted = 0,
        [Description("Success")]
        Success = 1,
        [Description("Failed")]
        Failed = 2,
        [Description("Warning")]
        Warning = 3
    }

    public class TupleList<T1, T2> : List<Tuple<T1, T2>>
    {
        public void Add(T1 item, T2 item2)
        {
            Add(new Tuple<T1, T2>(item, item2));
        }
    }

    public class FlowStepResult
    {
        public FlowStepResultValues ResultCode { get; set; }
        public Exception Exception { get; set; }
        public string Message { get; set; }
        public FlowStepResult(FlowStepResultValues res, Exception ex = null, string mes = null)
        {
            ResultCode = res;
            Exception = ex;
            Message = mes;
        }
        public override string ToString()
        {
            var result = "";
            if ((!string.IsNullOrEmpty(Message)))
                result += Message + "\r\n";
            if ((Exception != null))
                result += Exception.ToString() + "\r\n";
            return result;
        }
    }

    public class FlowStep
    {
        public string Id { get; set; }
        public FlowStepResult Result { get; set; }
    }

    public class Flow
    {
        internal FlowStep state;
        internal FlowEngine engine;
        internal Flow Run(Func<FlowStep, FlowStepResultValues> process)
        {
            state = new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) };
            state.Id = process.Method.Name + process.Method.Attributes.ToString();
            try
            {
                state.Result.ResultCode = process(state);
            }
            catch (Exception ex)
            {
                state.Result.Exception = ex;
                state.Result.ResultCode = FlowStepResultValues.Failed;
            }
            engine.Steps.Add(state);
            return new Flow(engine, state);
        }

        internal Flow Run(Action<FlowStep> process)
        {
            state = new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) };
            state.Id = process.Method.Name + process.Method.Attributes.ToString();
            try
            {
                process(state);
                state.Result.ResultCode = FlowStepResultValues.Success;
            }
            catch (Exception ex)
            {
                state.Result.Exception = ex;
                state.Result.ResultCode = FlowStepResultValues.Failed;
            }
            engine.Steps.Add(state);
            return new Flow(engine, state);
        }

        public Flow ContinueWith(Action<FlowStep> process)
        {
            return Run(process);
        }

        public FlowFork Where(TupleList<Func<FlowStep, bool>, Action<FlowStep>> conditionPairs)
        {
            List<Flow> wks = new List<Flow>();

            foreach (var pair in conditionPairs)
            {
                if (pair.Item1(state))
                    wks.Add(Run(pair.Item2));
            }
            return new FlowFork(engine, wks.Select(p => p.state).ToList()); 
        }

        public Flow Where(Func<FlowStep,bool> condition, Func<FlowStep, FlowStepResultValues> process)
        {
            if (condition(state))
                return Run(process);
            else
                return new Flow(this.engine, new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) });
        }

        public Flow IfFailed(Action<FlowStep> process)
        {
            if (state.Result.ResultCode == FlowStepResultValues.Failed)
                return Run(process);
            else
                return new Flow(this.engine, new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) });
        }

        public Flow IfNonExecuted(Action<FlowStep> process)
        {
            if (state.Result.ResultCode == FlowStepResultValues.NotExecuted)
                return Run(process);
            else
                return new Flow(this.engine, new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) });
        }

        public Flow IfFailed(Func<FlowStep, FlowStepResultValues> process)
        {
            if (state.Result.ResultCode == FlowStepResultValues.Failed)
                return Run(process);
            else
                return new Flow(this.engine, new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) });
        }

        public Flow IfSuccess(Action<FlowStep> process)
        {
            if (state.Result.ResultCode == FlowStepResultValues.Success)
                return Run(process);
            else
                return new Flow(this.engine, new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) });
        }

        public Flow IfSuccess(Func<FlowStep, FlowStepResultValues> process)
        {
            if (state.Result.ResultCode == FlowStepResultValues.Success)
                return Run(process);
            else
                return new Flow(this.engine, new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) });
        }

        public Flow IfWarning(Action<FlowStep> process)
        {
            if (state.Result.ResultCode == FlowStepResultValues.Warning)
                return Run(process);
            else
                return new Flow(this.engine, new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) });
        }

        public Flow IfWarning(Func<FlowStep, FlowStepResultValues> process)
        {
            if (state.Result.ResultCode == FlowStepResultValues.Warning)
                return Run(process);
            else
                return new Flow(this.engine, new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) });
        }

        internal Flow(FlowEngine eng, FlowStep aStep)
        {
            state = aStep;
            engine = eng;
        }

        internal Flow(FlowEngine eng)
        {
            engine = eng;
        }
    }

    public class FlowEngine
    {
        public List<FlowStep> Steps = new List<FlowStep>();

        public Flow Start(Func<FlowStep, FlowStepResultValues> process)
        {
            var eng = new Flow(this);
            eng.Run(process);
            return eng;
        }

        public Flow Start(Action<FlowStep> process)
        {
            var eng = new Flow(this);
            eng.Run(process);
            return eng;
        }

        public bool SomethingWasWrong()
        {
            return Steps.Exists(a => a.Result.ResultCode == FlowStepResultValues.Failed);
        }

        public bool AllWasGood()
        {
            return Steps.All(a => a.Result.ResultCode == FlowStepResultValues.Success);
        }

        public List<FlowStep> FailedSteps()
        {
            return Steps.Where(a => a.Result.ResultCode == FlowStepResultValues.Failed).ToList();
        }

        public string ErrorList()
        {
            return string.Join("\r\n", Steps.Where(a => a.Result.ResultCode == FlowStepResultValues.Failed).Select(e => e.Result.ToString()));
        }
    }

    public class FlowFork
    {
        internal List<FlowStep> _States;
        internal FlowEngine engine;
        internal Flow Run(Func<FlowStep, FlowStepResultValues> process)
        {
            FlowStep state = new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) };
            state.Id = process.Method.Name + process.Method.Attributes.ToString();
            try
            {
                state.Result.ResultCode = process(state);
            }
            catch (Exception ex)
            {
                state.Result.Exception = ex;
                state.Result.ResultCode = FlowStepResultValues.Failed;
            }
            engine.Steps.Add(state);
            return new Flow(engine, state);
        }

        internal Flow Run(Action<FlowStep> process)
        {
            FlowStep state = new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) };
            state.Id = process.Method.Name + process.Method.Attributes.ToString();
            try
            {
                process(state);
                state.Result.ResultCode = FlowStepResultValues.Success;
            }
            catch (Exception ex)
            {
                state.Result.Exception = ex;
                state.Result.ResultCode = FlowStepResultValues.Failed;
            }
            engine.Steps.Add(state);
            return new Flow(engine, state);
        }

        public Flow ContinueWith(Action<FlowStep> process)
        {
            return Run(process);
        }

        public Flow IfAnyFailed(Action<FlowStep> process)
        {
            if (_States.Any(r => r.Result.ResultCode == FlowStepResultValues.Failed))
                return Run(process);
            else
                return new Flow(this.engine, new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) });
        }

        public Flow IfAllFailed(Action<FlowStep> process)
        {
            if (_States.All(r => r.Result.ResultCode == FlowStepResultValues.Failed))
                return Run(process);
            else
                return new Flow(this.engine, new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) });
        }

        public Flow IfAnyWasNotExecuted(Action<FlowStep> process)
        {
            if (_States.Any(r => r.Result.ResultCode == FlowStepResultValues.NotExecuted))
                return Run(process);
            else
                return new Flow(this.engine, new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) });
        }

        public Flow IfAllSuccess(Action<FlowStep> process)
        {
            if (_States.All(r => r.Result.ResultCode == FlowStepResultValues.Success))
                return Run(process);
            else
                return new Flow(this.engine, new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) });
        }

        public Flow IfAnySuccess(Func<FlowStep, FlowStepResultValues> process)
        {
            if (_States.Any(r => r.Result.ResultCode == FlowStepResultValues.Success))
                return Run(process);
            else
                return new Flow(this.engine, new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) });
        }

        public Flow IfAnyWarning(Action<FlowStep> process)
        {
            if (_States.Any(r => r.Result.ResultCode == FlowStepResultValues.Warning))
                return Run(process);
            else
                return new Flow(this.engine, new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) });
        }

        public Flow IfAllWarning(Func<FlowStep, FlowStepResultValues> process)
        {
            if (_States.All(r => r.Result.ResultCode == FlowStepResultValues.Warning))
                return Run(process);
            else
                return new Flow(this.engine, new FlowStep() { Result = new FlowStepResult(FlowStepResultValues.NotExecuted) });
        }

        internal FlowFork(FlowEngine eng, List<FlowStep> states)
        {
            engine = eng;
            _States = states;
        }
    }

}

