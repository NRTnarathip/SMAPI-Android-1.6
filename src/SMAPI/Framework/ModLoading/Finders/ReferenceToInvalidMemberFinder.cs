using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.Framework.ModLoading.Framework;

namespace StardewModdingAPI.Framework.ModLoading.Finders;

/// <summary>Finds references to a field, property, or method which either doesn't exist or returns a different type than the code expects.</summary>
/// <remarks>This implementation is purely heuristic. It should never return a false positive, but won't detect all cases.</remarks>
internal class ReferenceToInvalidMemberFinder : BaseInstructionHandler
{
    /*********
    ** Fields
    *********/
    /// <summary>The assembly names to which to heuristically detect broken references.</summary>
    private readonly ISet<string> ValidateReferencesToAssemblies;

    /// <summary>Whether to include more technical details about broken mods in the TRACE logs. This is mainly useful for creating compatibility rewriters.</summary>
    private readonly bool LogTechnicalDetailsForBrokenMods;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="validateReferencesToAssemblies">The assembly names to which to heuristically detect broken references.</param>
    /// <param name="logTechnicalDetailsForBrokenMods">Whether to include more technical details about broken mods in the TRACE logs. This is mainly useful for creating compatibility rewriters.</param>
    public ReferenceToInvalidMemberFinder(ISet<string> validateReferencesToAssemblies, bool logTechnicalDetailsForBrokenMods)
        : base(defaultPhrase: "")
    {
        this.ValidateReferencesToAssemblies = validateReferencesToAssemblies;
        this.LogTechnicalDetailsForBrokenMods = logTechnicalDetailsForBrokenMods;
    }

    /// <inheritdoc />
    public override bool Handle(ModuleDefinition module, ILProcessor cil, Instruction instruction)
    {
        // field reference
        FieldReference? fieldRef = RewriteHelper.AsFieldReference(instruction);
        if (fieldRef != null && this.ShouldValidate(fieldRef.DeclaringType))
        {
            FieldDefinition? targetField = fieldRef.DeclaringType.Resolve()?.Fields.FirstOrDefault(p => p.Name == fieldRef.Name);

            // wrong return type
            if (targetField != null &&
                !RewriteHelper.LooksLikeSameType(fieldRef.FieldType, targetField.FieldType))
            {
                string fieldReturnsText = $"field returns {targetField.FieldType}";
                this.MarkFlag(InstructionHandleResult.NotCompatible,
                    $"reference to {this.GetMemberDisplayName(fieldRef)} ({fieldReturnsText}, not {fieldRef.FieldType})");
            }
            //if (targetField != null && !RewriteHelper.LooksLikeSameType(fieldRef.FieldType, targetField.FieldType))
            //    this.MarkFlag(InstructionHandleResult.NotCompatible, $"reference to {this.GetMemberDisplayName(fieldRef)} (field returns {this.GetFriendlyTypeName(targetField.FieldType)}, not {this.GetFriendlyTypeName(fieldRef.FieldType)})");

            // missing
            else if (targetField == null || targetField.HasConstant || !RewriteHelper.HasSameNamespaceAndName(fieldRef.DeclaringType, targetField.DeclaringType))
                this.MarkFlag(InstructionHandleResult.NotCompatible, $"reference to {this.GetMemberDisplayName(fieldRef)} (no such field)");

            return false;
        }

        // method reference
        MethodReference? methodRef = RewriteHelper.AsMethodReference(instruction);
        if (methodRef != null && !this.IsUnsupported(methodRef))
        {
            MethodDefinition? methodDef = methodRef.Resolve();

            // wrong return type
            if (methodDef != null && this.ShouldValidate(methodRef.DeclaringType))
            {
                MethodDefinition[]? candidateMethods = methodRef.DeclaringType.Resolve()?.Methods.Where(found => found.Name == methodRef.Name).ToArray();
                if (candidateMethods?.Any() is true && candidateMethods.All(method => !RewriteHelper.LooksLikeSameType(method.ReturnType, methodDef.ReturnType)))
                    this.MarkFlag(InstructionHandleResult.NotCompatible, $"reference to {this.GetMemberDisplayName(methodDef)} (no such method returns {this.GetFriendlyTypeName(methodDef.ReturnType)})");
            }

            // missing
            else if (methodDef is null)
            {
                string typeName;
                if (this.IsProperty(methodRef))
                    typeName = "property";
                else if (methodRef.Name == ".ctor")
                    typeName = "constructor";
                else
                    typeName = "method";

                this.MarkFlag(InstructionHandleResult.NotCompatible, $"reference to {this.GetMemberDisplayName(methodRef)} (no such {typeName})");
            }
        }

        return false;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Whether references to the given type should be validated.</summary>
    /// <param name="type">The type reference.</param>
    private bool ShouldValidate([NotNullWhen(true)] TypeReference? type)
    {
        return type != null && this.ValidateReferencesToAssemblies.Contains(type.Scope.Name);
    }

    /// <summary>Get whether a method reference is a special case that's not currently supported (e.g. array methods).</summary>
    /// <param name="method">The method reference.</param>
    private bool IsUnsupported(MethodReference method)
    {
        return
            method.DeclaringType.Name.Contains("["); // array methods
    }

    /// <summary>Get the member name to show in logged messages.</summary>
    /// <param name="memberRef">The member reference.</param>
    private string GetMemberDisplayName(MemberReference memberRef)
    {
        if (this.LogTechnicalDetailsForBrokenMods)
            return memberRef.FullName;

        string name = memberRef.Name;
        if (memberRef is PropertyReference)
            name = name[4..]; // remove `get_` or `set_` prefix

        return $"{memberRef.DeclaringType.FullName}.{name}";
    }

    /// <summary>Get a shorter type name for display.</summary>
    /// <param name="type">The type reference.</param>
    private string GetFriendlyTypeName(TypeReference type)
    {
        // most common built-in types
        switch (type.FullName)
        {
            case "System.Boolean":
                return "bool";
            case "System.Int32":
                return "int";
            case "System.String":
                return "string";
        }

        // most common unambiguous namespaces
        foreach (string @namespace in new[] { "Microsoft.Xna.Framework", "Netcode", "System", "System.Collections.Generic" })
        {
            if (type.Namespace == @namespace)
                return type.Name;
        }

        return type.FullName;
    }

    /// <summary>Get whether a method reference is a property getter or setter.</summary>
    /// <param name="method">The method reference.</param>
    private bool IsProperty(MethodReference method)
    {
        return method.Name.StartsWith("get_") || method.Name.StartsWith("set_");
    }
}
