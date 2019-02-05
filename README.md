# Mini Running Work Flow Fluent code.

nRunFlow is an small utility for implement a desition tree, organizing a set of actions that will be executed according to certain conditions. Most common conditions is Fail or Success of a previous action.

### FlowChart Example

```flow
st=>start: Set Up Variables
op1=>operation: Check Service Status
op2=>operation: Populate Dependencies
op3=>operation: Process the data
cond0=>condition: Successful
cond1=>condition: Successful
cond2=>condition: Successful
e=>end: Alternative Route

st->op1->cond1->op2->cond2
cond1(yes)->op2
cond1(no)->e
cond2(yes)->op3

```

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
	 .ContinueWith(s => {               // Continue Always; Fail or Success the prev step.
			CheckServiceStatus();
	 })
	.ContinueIf(s => {
			PopulateDependencies();  // If Success Action
	}, s => {
			AlternativeRoute();            // Else Action (Fail/Warning/NonExecuted)
	})
	.IfSuccess(s => {                        // If Success Action
			ProcessData();
	})
```

### Links

`<github>` : <https://github.com/israelito3000/nRunFlow>

