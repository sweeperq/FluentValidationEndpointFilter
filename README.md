# Fluent Validation Endpoint Filter

## Backstory
We often find ourselves needing to perform validation on the models being submitted to our Minimal APIs. Traditionally, we would have something like the following:

```
app.MapPost(string.Empty, async (AppDbContext db, BrandCreateModel model, IValidator<BrandCreateModel> validator) => {
   var valResult = await validator.ValidateAsync(model);
   if (!valResult.IsValid) {
     return Results.ValidationProblem(result.ToDictionary(), statusCode: StatusCodes.Status422UnprocessableEntity);
   }
   var brand = new Brand() { Name = model.Name, Description = model.Description };
   db.Brands.Add(brand);
   await db.SaveChangesAsync();
   return Results.Created($"/api/brands/{brand.Id}", brand);
})
  .Produces(StatusCodes.Status422UnprocessableEntity)
  .Produces<Brand>(StatusCodes.Status201Created);
```

There isn't anything wrong with this. It is functional and understandable. However, repeating the validation
logic in every POST and PUT method felt very repetitive. We figured there had to be a layer we could take
advantage of to reduce this duplication. What we eventually discovered was the `IEndpointFilter` interface.

`IEndpointFilter` allows you to create filters that run prior to the body of your Minimal API methods. Sound
like a great place to do validation, right?!

After some Googling, we discovered this wasn't a new problem. [Ben Foster](https://benfoster.io/blog/minimal-api-validation-endpoint-filters/)
came up with a solution in 2022 for .Net 7 minimal APIs. We've studied that solution, made "improvements"
utilizing features available in .Net 9, and are publishing a NuGet package making it easy for others to implement.
If your org doesn't like 3rd-party NuGets, or you don't like how something works, the source is right here.

## Usage

Usage is straight-forward. Create your models and validators, then register them with Dependency Injection like you
normally would. Then it is just two simple steps...

1. Download the NuGet package
1. Add `.Validate<ModelType>()` to the end of your API method you would like to provide validation for

Here is the simplified version of the API definition at the beginning of this document:

```
app.MapPost(string.Empty, async (AppDbContext db, BrandCreateModel model) => {
   var brand = new Brand() { Name = model.Name, Description = model.Description };
   db.Brands.Add(brand);
   await db.SaveChangesAsync();
   return Results.Created($"/api/brands/{brand.Id}", brand);
})
  .Validate<BrandCreateModel>()
  .Produces<Brand>(StatusCodes.Status201Created);
```

It is important to note that `Validate<BrandCreateModel>()` not only performs the validation, but
it also checks to make sure the model is present, and sets the possible Open API return types. For
instance, if the body is missing from the request and the model ends up being null, it produces a
`BadRequest<string>` with the default message "Request body is missing and cannot be validated". If
the validation fails, it produces `UnprocessibleEntity<ValidationProblemDetails>`.