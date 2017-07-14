namespace Platform.Utils.Events.QueryGenerator
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using Newtonsoft.Json.Linq;
    using Platform.Utils.Domain.Objects;
    using Platform.Utils.Events.Domain.Enums;
    using Platform.Utils.Events.Domain.Objects;
    using Platform.Utils.Events.QueryGenerator.Domain;
    using Platform.Utils.Events.QueryGenerator.Extensions;
    using Platform.Utils.Events.QueryGenerator.Helpers;
    using Platform.Utils.Events.QueryGenerator.Interfaces;
    using Platform.Utils.Events.QueryParser.Domain.Enums;
    using Platform.Utils.Events.QueryParser.Domain.Objects;
    using Platform.Utils.Events.QueryParser.Helpers;
    using Platform.Utils.Json;

    public class DynamicApiWarder
    {
        private readonly IModelElementStorage modelElementStorage;

        public DynamicApiWarder(IModelElementStorage modelElementStorage)
        {
            this.modelElementStorage = modelElementStorage;
        }

        public ExecutionResult<ApiActionInfo> HandleRequest(string requestUri, 
            JObject body, 
            HttpVerbs verb,
            bool useDots = true)
        {
            var result = new ApiActionInfo();

            string tags = requestUri;

            var edmObject = new JObject();
            var currentNode = edmObject;
            ModelElementObjectBase currentElement = null;
            PivotDefinition pivot = null;
            bool isProperty = false;
            bool isId = false;

            foreach (var tag in tags.Split(new [] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                Guid id;
                if (Guid.TryParse(tag, out id))
                {
                    isProperty = false;
                    isId = true;
                    currentNode.Add(ObjectHelper.IdPropName, id);
                }
                else
                {
                    if (result.PathItems.Any() == false)
                    {
                        currentElement = this.modelElementStorage.GetModelElementTree(tag);

                        if (currentElement == null)
                        {
                            return ExecutionResult.ErrorResultFor(result, "404", $"Uri must starts from {currentElement.DisplayText}, but {tag} found.");
                        }

                        result.PathItems.Add(currentElement.DisplayText);
                    }
                    else
                    {
                        //parent tag
                        if (pivot.Type != PivotType.BusinessProcess && pivot.IsIntent && isProperty)
                        {
                            return ExecutionResult.ErrorResultFor(result, "404", $"Must be id after {currentElement.DisplayText}, found {tag}.");
                        }

                        currentElement = currentElement.FindByPath(tag, useDots);

                        if (currentElement == null)
                        {
                            return ExecutionResult.ErrorResultFor(result, "404", $"There no property {tag}.");
                        }

                        result.PathItems.Add(currentElement.DisplayText);
                    }

                    isProperty = true;
                    isId = false;

                    pivot = ParserHelper.GetPivot(ParserHelper.TreeNodeTypeToPivot[currentElement.ModelElementType]);

                    if (pivot.IsCollection)
                    {
                        var newNode = new JObject();
                        currentNode.Add(currentElement.DisplayText, new JArray(newNode));
                        currentNode = newNode;
                    }
                    else
                    {
                        var newNode = new JObject();
                        currentNode.Add(currentElement.DisplayText, newNode);
                        currentNode = newNode;
                    }
                }
            }

            if (pivot.Type == PivotType.Value && isProperty)
            {
                return ExecutionResult.ErrorResultFor(result, "404", $"Must have id after {currentElement.DisplayText}, found.");
            }

            result.EdmObject = edmObject;
            result.EventAction = DynamicApiHelper.GetEventAction(verb, pivot.Type, isId);

            if ((result.EventAction == EventAction.Add || result.EventAction == EventAction.Initiate ||
                result.EventAction == EventAction.Set) && body == null)
            {
                return ExecutionResult.ErrorResultFor(result, "404", $"Request body must not be null for {result.EventAction}.");
            }

            if (body != null)
            {
                foreach (var property in body.Properties())
                {
                    currentNode.Add(property.Name, property.Value);
                }

                if (DynamicApiHelper.MustHaveOperationInEdm(result.EventAction))
                {
                    currentNode.Add("$operation", result.EventAction.ToString().ToLowerInvariant());
                }
            }
            
            result.PlaceForBody = currentNode;

            return ExecutionResult.SuccessResult(result);
        }
    }
}