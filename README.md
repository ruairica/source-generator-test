Playing around with source generators.

The Generator project is a source generator that finds all dependencies that are injected in the WorkerExample project
and statically generates an extension method "AddGeneratedDependencies". This extension method essentially is "builder.Services.AddTransient<T,U>();"
for all injected dependencies. The method is called in Program.cs in WorkerExample
