using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    /// <summary>
    /// Any concrete implementation of this class can provide shapes for any content item which has a specific Part.
    /// </summary>
    /// <typeparam name="TPart"></typeparam>
    public abstract class ContentPartDisplayDriver<TPart> : DisplayDriverBase, IContentPartDisplayDriver where TPart : ContentPart, new()
    {
        private ContentTypePartDefinition _typePartDefinition;

        public override ShapeResult Shape(string shapeType, Func<IBuildShapeContext, Task<IShape>> shapeBuilder, Func<IShape, Task> initializeAsync)
        {
            // e.g., BodyPart.Summary, BodyPart-BlogPost, BagPart-LandingPage-Services
            // context.Shape is the ContentItem shape, we need to alter the part shape

            var result = base.Shape(shapeType, shapeBuilder, initializeAsync);

            if (_typePartDefinition != null)
            {
                result.Name(_typePartDefinition.Name);

                result.Displaying(ctx =>
                {
                    // [PartType]_[DisplayType], e.g. BodyPart.Summary
                    ctx.ShapeMetadata.Alternates.Add($"{_typePartDefinition.PartDefinition.Name}_{ctx.ShapeMetadata.DisplayType}");

                    // [PartType]__[ContentType]
                    ctx.ShapeMetadata.Alternates.Add($"{_typePartDefinition.PartDefinition.Name}__{_typePartDefinition.ContentTypeDefinition.Name}");

                    // [PartType]_[DisplayType]__[ContentType], e.g. BodyPart-Blog.Summary
                    ctx.ShapeMetadata.Alternates.Add($"{_typePartDefinition.PartDefinition.Name}_{ctx.ShapeMetadata.DisplayType}__{_typePartDefinition.ContentTypeDefinition.Name}");

                    // Named part have 
                    // - [PartType]__[ContentType]__[PartName], e.g. BagPart-LandingPage-Features
                    // - [PartType]_[DisplayType]__[ContentType]__[PartName], e.g. BagPart-LandingPage-Features.Summary
                    if (_typePartDefinition.PartDefinition.Name != _typePartDefinition.Name)
                    {
                        ctx.ShapeMetadata.Alternates.Add($"{_typePartDefinition.PartDefinition.Name}__{_typePartDefinition.ContentTypeDefinition.Name}__{_typePartDefinition.Name}");
                        ctx.ShapeMetadata.Alternates.Add($"{_typePartDefinition.PartDefinition.Name}_{ctx.ShapeMetadata.DisplayType}__{_typePartDefinition.ContentTypeDefinition.Name}__{_typePartDefinition.Name}");
                    }
                });
            }

            return result;
        }

        Task<IDisplayResult> IContentPartDisplayDriver.BuildDisplayAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, BuildDisplayContext context)
        {
            var part = contentPart as TPart;

            if (part == null)
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            BuildPrefix(typePartDefinition, context.HtmlFieldPrefix);

            _typePartDefinition = typePartDefinition;

            var buildDisplayContext = new BuildPartDisplayContext(typePartDefinition, context);

            return DisplayAsync(part, buildDisplayContext);
        }

        Task<IDisplayResult> IContentPartDisplayDriver.BuildEditorAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, BuildEditorContext context)
        {
            var part = contentPart as TPart;

            if (part == null)
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            BuildPrefix(typePartDefinition, context.HtmlFieldPrefix);

            var buildEditorContext = new BuildPartEditorContext(typePartDefinition, context);

            return EditAsync(part, buildEditorContext);
        }

        Task<IDisplayResult> IContentPartDisplayDriver.UpdateEditorAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, UpdateEditorContext context)
        {
            var part = contentPart as TPart;

            if(part == null)
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            BuildPrefix(typePartDefinition, context.HtmlFieldPrefix);

            var updateEditorContext = new UpdatePartEditorContext(typePartDefinition, context);

            var result = UpdateAsync(part, context.Updater, updateEditorContext);

            part.ContentItem.Apply(typePartDefinition.Name, part);
            
            return result;
        }

        public virtual Task<IDisplayResult> DisplayAsync(TPart part, BuildPartDisplayContext context)
        {
            return Task.FromResult(Display(part, context));
        }

        public virtual IDisplayResult Display(TPart part, BuildPartDisplayContext context)
        {
            return Display(part);
        }

        public virtual IDisplayResult Display(TPart part)
        {
            return null;
        }

        public virtual Task<IDisplayResult> EditAsync(TPart part, BuildPartEditorContext context)
        {
            return Task.FromResult(Edit(part, context));
        }

        public virtual IDisplayResult Edit(TPart part, BuildPartEditorContext context)
        {
            return Edit(part);
        }

        public virtual IDisplayResult Edit(TPart part)
        {
            return null;
        }

        public virtual Task<IDisplayResult> UpdateAsync(TPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            return UpdateAsync(part, context);
        }

        public virtual Task<IDisplayResult> UpdateAsync(TPart part, BuildPartEditorContext context)
        {
            return UpdateAsync(part, context.Updater);
        }

        public virtual Task<IDisplayResult> UpdateAsync(TPart part, IUpdateModel updater)
        {
            return Task.FromResult<IDisplayResult>(null);
        }

        private void BuildPrefix(ContentTypePartDefinition typePartDefinition, string htmlFieldPrefix)
        {
            Prefix = typePartDefinition.Name;

            if (!String.IsNullOrEmpty(htmlFieldPrefix))
            {
                Prefix = htmlFieldPrefix + "." + Prefix;
            }
        }
    }
}
