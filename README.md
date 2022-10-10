# UiPathTeam.RulesEngine
UiPath Rules Engine is a library/NuGet package for abstracting business logic/rules/policies out of a workflow.

Every developer in enterprise writes the logic in workflow using many if-else conditions and loops which complicate workflow and make it hard to debug and trace issue.

Problem:
An organization might make hundreds of rules-based decisions as part of everyday business. While automation robots can execute such decisions, the increasing volume and complexity of data-based decisions make it prudent to find a solution that can eliminate the risk and make these rules centered and easy to maintain.​

A Developer in an enterprise writes the logic in the automation workflow using many if-else conditions and loops, which complicate workflow and make it hard to debug and trace issues.


<B>Solution</B>
Using a business rules engine (BRE) that automates decision-making when there are specific rules, such as government regulations or organizational security policies, to streamline decision-making, by having a rule engine activity, we can simplify the code and gain an advantage over our competitors.

BRE operates via a relatively straightforward input/output mechanism: It gets notified of the need for a decision via input data sets, evaluates that data against defined rules, and then delivers the information it derives in the form of a decision.
Proposed solution is to build custom activity that acts as an enterprise-level management platform for organizational policies & Rules, equipped with faculties such as:
High-level rule authoring tools
A graphics-based user interface
A rules repository with version control and rollback capabilities
Rule-testing and simulation functions; and ogs and records of decisions made over time

<b>Solution Architecture:</b>
![image](https://user-images.githubusercontent.com/11008302/194887211-5fd5b9c2-b211-4fd2-8646-b4b81f7fba8d.png)
