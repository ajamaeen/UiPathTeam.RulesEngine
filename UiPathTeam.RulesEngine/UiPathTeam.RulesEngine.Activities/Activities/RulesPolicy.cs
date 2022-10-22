using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using UiPathTeam.RulesEngine.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using System.IO;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Xml;
using System.Workflow.Activities.Rules;
using UiPath.Shared.Localization;

namespace UiPathTeam.RulesEngine.Activities
{
    /// <summary>
    /// This activity allows opening an existing .rules file
    /// and execute one of its rules against a TargetObject.
    /// Users can modify the ruleset (add, update, delete rules).
    [LocalizedDisplayName(nameof(Resources.RulesPolicy_DisplayName))]
    [LocalizedDescription(nameof(Resources.RulesPolicy_Description))]
    public class RulesPolicy<T> : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        /// <summary>
        /// Rules file path (fileName.rules)
        /// </summary>
        [RequiredArgument]
        [LocalizedDisplayName(nameof(Resources.RulesPolicy_RulesFilePath_DisplayName))]
        [LocalizedDescription(nameof(Resources.RulesPolicy_RulesFilePath_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> RulesFilePath { get; set; }

        /// <summary>
        /// Rule set name
        /// </summary>
        [RequiredArgument]
        [LocalizedDisplayName(nameof(Resources.RulesPolicy_RuleSetName_DisplayName))]
        [LocalizedDescription(nameof(Resources.RulesPolicy_RuleSetName_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> RuleSetName { get; set; }

        /// <summary>
        /// An object to apply rules on.
        /// </summary>
        [RequiredArgument]
        [LocalizedDisplayName(nameof(Resources.RulesPolicy_TargetObject_DisplayName))]
        [LocalizedDescription(nameof(Resources.RulesPolicy_TargetObject_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<T> TargetObject { get; set; }


        /// <summary>
        /// Result object after applying the rules.
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.RulesPolicy_ResultObject_DisplayName))]
        [LocalizedDescription(nameof(Resources.RulesPolicy_ResultObject_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<T> ResultObject { get; set; }


        /// <summary>
        /// Holds a collection of System.Workflow.ComponentModel.Compiler.ValidationError
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.RulesPolicy_ValidationErrors_DisplayName))]
        [LocalizedDescription(nameof(Resources.RulesPolicy_ValidationErrors_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<ValidationErrorCollection> ValidationErrors { get; set; }

        #endregion


        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public RulesPolicy()
        {
        }

        #endregion

        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            if (RulesFilePath == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(RulesFilePath)));
            if (RuleSetName == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(RuleSetName)));
            if (TargetObject == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(TargetObject)));            
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Inputs
            var rulesfilepath = RulesFilePath.Get(context);
            var rulesetname = RuleSetName.Get(context);
            var targetobject = TargetObject.Get(context);

            if (string.IsNullOrWhiteSpace(rulesfilepath) || string.IsNullOrWhiteSpace(rulesetname))
            {
                throw new InvalidOperationException(SharedResources.RuleFilePathNotSet);
            }

            if (!File.Exists(rulesfilepath))
            {
                throw new InvalidOperationException(SharedResources.RulesFileNotFound);
            }

            // Get the RuleSet from the .rules file
            WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
            XmlTextReader reader = new XmlTextReader(rulesfilepath);
            RuleDefinitions rules = serializer.Deserialize(reader) as RuleDefinitions;
            RuleSet ruleSet = rules.RuleSets[rulesetname];

            if (ruleSet == null)
            {
                throw new InvalidOperationException(String.Format(SharedResources.RuleSetNotFoundInFile,rulesetname,rulesfilepath));
            }
            
            // Validate before running
            Type targetType = this.TargetObject.Get(context).GetType();
            RuleValidation validation = new RuleValidation(targetType, null);
            if (!ruleSet.Validate(validation))
            {
                // Set the ValidationErrors OutArgument
                this.ValidationErrors.Set(context, validation.Errors);

                // Throw exception
                throw new ValidationException(string.Format(SharedResources.RulesetIsNotValid, validation.Errors.Count));
            }

            // Execute the ruleset
            object evaluatedTarget = this.TargetObject.Get(context);
            RuleEngine engine = new RuleEngine(ruleSet, validation);
            engine.Execute(evaluatedTarget);

            // Update the Result object
            //this.ResultObject.Set(context, evaluatedTarget);

            // Outputs
            return (ctx) =>
            {
                ResultObject.Set(ctx, evaluatedTarget);
                ValidationErrors.Set(ctx, validation.Errors);
            };
        }

        #endregion
    }
}

