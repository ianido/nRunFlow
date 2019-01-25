using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace Workflow
{
    public enum WorkFlowStepResultValues
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

    public class WorkFlowStepResult
    {
        public WorkFlowStepResultValues ResultCode { get; set; }
        public Exception Exception { get; set; }
        public string Message { get; set; }
        public WorkFlowStepResult(WorkFlowStepResultValues res, Exception ex = null, string mes = null)
        {
            ResultCode = res;
            Exception = ex;
            Message = mes;
        }
    }

    public class WorkFlowStep
    {
        public string Id { get; set; }
        public WorkFlowStepResult Result { get; set; }
    }

    public class WorkFlow
    {
        internal WorkFlowStep state;
        internal WorkFlowEngine engine;
        internal WorkFlow Run(Func<WorkFlowStep, WorkFlowStepResultValues> process)
        {
            state = new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) };
            state.Id = process.Method.Name + process.Method.Attributes.ToString();
            try
            {
                state.Result.ResultCode = process(state);
            }
            catch (Exception ex)
            {
                state.Result.Exception = ex;
                state.Result.ResultCode = WorkFlowStepResultValues.Failed;
            }
            engine.Steps.Add(state);
            return new WorkFlow(engine, state);
        }

        internal WorkFlow Run(Action<WorkFlowStep> process)
        {
            state = new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) };
            state.Id = process.Method.Name + process.Method.Attributes.ToString();
            try
            {
                process(state);
                state.Result.ResultCode = WorkFlowStepResultValues.Success;
            }
            catch (Exception ex)
            {
                state.Result.Exception = ex;
                state.Result.ResultCode = WorkFlowStepResultValues.Failed;
            }
            engine.Steps.Add(state);
            return new WorkFlow(engine, state);
        }

        public WorkFlow ContinueWith(Action<WorkFlowStep> process)
        {
            return Run(process);
        }

        public WorkFlowFork Where(TupleList<Func<WorkFlowStep, bool>, Action<WorkFlowStep>> conditionPairs)
        {
            List<WorkFlow> wks = new List<WorkFlow>();

            foreach (var pair in conditionPairs)
            {
                if (pair.Item1(state))
                    wks.Add(Run(pair.Item2));
            }
            return new WorkFlowFork(engine, wks.Select(p => p.state).ToList()); 
        }

        public WorkFlow Where(Func<WorkFlowStep,bool> condition, Func<WorkFlowStep, WorkFlowStepResultValues> process)
        {
            if (condition(state))
                return Run(process);
            else
                return new WorkFlow(this.engine, new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) });
        }

        public WorkFlow IfFailed(Action<WorkFlowStep> process)
        {
            if (state.Result.ResultCode == WorkFlowStepResultValues.Failed)
                return Run(process);
            else
                return new WorkFlow(this.engine, new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) });
        }

        public WorkFlow IfNonExecuted(Action<WorkFlowStep> process)
        {
            if (state.Result.ResultCode == WorkFlowStepResultValues.NotExecuted)
                return Run(process);
            else
                return new WorkFlow(this.engine, new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) });
        }

        public WorkFlow IfFailed(Func<WorkFlowStep, WorkFlowStepResultValues> process)
        {
            if (state.Result.ResultCode == WorkFlowStepResultValues.Failed)
                return Run(process);
            else
                return new WorkFlow(this.engine, new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) });
        }

        public WorkFlow IfSuccess(Action<WorkFlowStep> process)
        {
            if (state.Result.ResultCode == WorkFlowStepResultValues.Success)
                return Run(process);
            else
                return new WorkFlow(this.engine, new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) });
        }

        public WorkFlow IfSuccess(Func<WorkFlowStep, WorkFlowStepResultValues> process)
        {
            if (state.Result.ResultCode == WorkFlowStepResultValues.Success)
                return Run(process);
            else
                return new WorkFlow(this.engine, new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) });
        }

        public WorkFlow IfWarning(Action<WorkFlowStep> process)
        {
            if (state.Result.ResultCode == WorkFlowStepResultValues.Warning)
                return Run(process);
            else
                return new WorkFlow(this.engine, new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) });
        }

        public WorkFlow IfWarning(Func<WorkFlowStep, WorkFlowStepResultValues> process)
        {
            if (state.Result.ResultCode == WorkFlowStepResultValues.Warning)
                return Run(process);
            else
                return new WorkFlow(this.engine, new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) });
        }

        internal WorkFlow(WorkFlowEngine eng, WorkFlowStep aStep)
        {
            state = aStep;
            engine = eng;
        }

        internal WorkFlow(WorkFlowEngine eng)
        {
            engine = eng;
        }
    }

    public class WorkFlowEngine
    {
        public List<WorkFlowStep> Steps = new List<WorkFlowStep>();

        public WorkFlow Start(Func<WorkFlowStep, WorkFlowStepResultValues> process)
        {
            var eng = new WorkFlow(this);
            eng.Run(process);
            return eng;
        }

        public WorkFlow Start(Action<WorkFlowStep> process)
        {
            var eng = new WorkFlow(this);
            eng.Run(process);
            return eng;
        }

        public bool SomethingWasWrong()
        {
            return Steps.Exists(a => a.Result.ResultCode == WorkFlowStepResultValues.Failed);
        }

        public bool AllWasGood()
        {
            return Steps.All(a => a.Result.ResultCode == WorkFlowStepResultValues.Success);
        }
    }

    public class WorkFlowFork
    {
        internal List<WorkFlowStep> _States;
        internal WorkFlowEngine engine;
        internal WorkFlow Run(Func<WorkFlowStep, WorkFlowStepResultValues> process)
        {
            WorkFlowStep state = new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) };
            state.Id = process.Method.Name + process.Method.Attributes.ToString();
            try
            {
                state.Result.ResultCode = process(state);
            }
            catch (Exception ex)
            {
                state.Result.Exception = ex;
                state.Result.ResultCode = WorkFlowStepResultValues.Failed;
            }
            engine.Steps.Add(state);
            return new WorkFlow(engine, state);
        }

        internal WorkFlow Run(Action<WorkFlowStep> process)
        {
            WorkFlowStep state = new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) };
            state.Id = process.Method.Name + process.Method.Attributes.ToString();
            try
            {
                process(state);
                state.Result.ResultCode = WorkFlowStepResultValues.Success;
            }
            catch (Exception ex)
            {
                state.Result.Exception = ex;
                state.Result.ResultCode = WorkFlowStepResultValues.Failed;
            }
            engine.Steps.Add(state);
            return new WorkFlow(engine, state);
        }

        public WorkFlow ContinueWith(Action<WorkFlowStep> process)
        {
            return Run(process);
        }

        public WorkFlow IfAnyFailed(Action<WorkFlowStep> process)
        {
            if (_States.Any(r => r.Result.ResultCode == WorkFlowStepResultValues.Failed))
                return Run(process);
            else
                return new WorkFlow(this.engine, new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) });
        }

        public WorkFlow IfAllFailed(Action<WorkFlowStep> process)
        {
            if (_States.All(r => r.Result.ResultCode == WorkFlowStepResultValues.Failed))
                return Run(process);
            else
                return new WorkFlow(this.engine, new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) });
        }

        public WorkFlow IfAnyWasNotExecuted(Action<WorkFlowStep> process)
        {
            if (_States.Any(r => r.Result.ResultCode == WorkFlowStepResultValues.NotExecuted))
                return Run(process);
            else
                return new WorkFlow(this.engine, new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) });
        }

        public WorkFlow IfAllSuccess(Action<WorkFlowStep> process)
        {
            if (_States.All(r => r.Result.ResultCode == WorkFlowStepResultValues.Success))
                return Run(process);
            else
                return new WorkFlow(this.engine, new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) });
        }

        public WorkFlow IfAnySuccess(Func<WorkFlowStep, WorkFlowStepResultValues> process)
        {
            if (_States.Any(r => r.Result.ResultCode == WorkFlowStepResultValues.Success))
                return Run(process);
            else
                return new WorkFlow(this.engine, new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) });
        }

        public WorkFlow IfAnyWarning(Action<WorkFlowStep> process)
        {
            if (_States.Any(r => r.Result.ResultCode == WorkFlowStepResultValues.Warning))
                return Run(process);
            else
                return new WorkFlow(this.engine, new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) });
        }

        public WorkFlow IfAllWarning(Func<WorkFlowStep, WorkFlowStepResultValues> process)
        {
            if (_States.All(r => r.Result.ResultCode == WorkFlowStepResultValues.Warning))
                return Run(process);
            else
                return new WorkFlow(this.engine, new WorkFlowStep() { Result = new WorkFlowStepResult(WorkFlowStepResultValues.NotExecuted) });
        }

        internal WorkFlowFork(WorkFlowEngine eng, List<WorkFlowStep> states)
        {
            engine = eng;
            _States = states;
        }
    }

}

