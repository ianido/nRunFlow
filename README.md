# Mini Running Work Flow Fluent code.

nRunFlow is an small utility for implement a simple decision chart, organizing a set of actions that will be executed according to certain conditions. Most common conditions is Fail or Success of a previous action, but this also can be extended.

### FlowChart Example

<img src="https://github.com/israelito3000/nRunFlow/blob/master/flow.JPG" width="30%"/>

### Features
- Fluent syntax.
- Easy to setup
- Easy to extend.


### Install and How tu Use

#### Install Nuget Package

`$ npm install nRunFlow`

#### How to use

``` javascript
var eng = new FlowEngine();

eng.Start(s => {
       SetupVariables();
   })
   .ContinueWith(s => {         // Continue Always; Fail or Success the prev step.
       CheckServiceStatus();
   })
   .ContinueIf(s => {
       PopulateDependencies();  // If Success Action
   }, s => {
       AlternativeRoute();      // Else Action (Fail/Warning/NonExecuted)
   })
   .IfSuccess(s => {            // If Success Action
       ProcessData();
   })
```
#### Supported Operations

- IfFail (action)
> Continue to execute the <action> if the previous step Fail
- IfSuccess (action)
> Continue to execute the <action> if the previous step Succeed
- IfWarning (action)
> Continue to execute the <action> if the previous step mark result as Warning       
- IfNonExecuted (action)
> Continue to execute the <action> if the previous step mark result as NonExecuted. (steps can internally modify their result)       
- ContinueWith (action)
> Continue to execute the <action> no matter the result of previous step       
- ContinueIf (actionIfSuccess, actionElse)
> Continue to execute the <actionIfSuccess> if the previous step Succeed else execute actionElse       
- Where (condition, action)
> Continue to execute the <action> if the condition is true       
- Where (TupleList<condition, action>)
> Continue to execute the "action" if the paired "condition" is True   
       
#### About Actions
Each action may receive the instance of the current step, so each action can change the result of the step, by default if an exception occur in the action and is not catched, the step result is automatically set as Failed.

### Links

`<github>` : <https://github.com/israelito3000/nRunFlow>

