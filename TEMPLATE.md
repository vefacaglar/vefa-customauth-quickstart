# Vefa CustomAuth Template

Install this repository as a local .NET template:

```bash
dotnet new install .
```

Generate a new solution:

```bash
dotnet new vefa-customauth -n MyCompany.AuthDemo
cd MyCompany.AuthDemo
scripts/setup-dev.sh
scripts/run-all.sh
```

The `-n` value replaces the root namespace and solution name. For example,
`MyCompany.AuthDemo` generates `MyCompany.AuthDemo.slnx` and namespaces such as
`MyCompany.AuthDemo.AuthServer`.
