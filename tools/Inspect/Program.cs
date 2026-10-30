using Mono.Cecil;
using System.Text.RegularExpressions;
if(args.Length<3) { Console.WriteLine("Inspect <assembly> <type-regex> <method-regex> [--il]"); return; }
using var assembly=AssemblyDefinition.ReadAssembly(args[0]);
IEnumerable<TypeDefinition> Types(IEnumerable<TypeDefinition> types) => types.SelectMany(t=>new[]{t}.Concat(Types(t.NestedTypes)));
if(args.Contains("--callers")) {
 foreach(var type in Types(assembly.MainModule.Types)) foreach(var method in type.Methods.Where(m=>m.HasBody))
  if(method.Body.Instructions.Any(i=>Regex.IsMatch(i.Operand?.ToString()??"",args[2],RegexOptions.IgnoreCase))) Console.WriteLine(method.FullName);
 return;
}
foreach(var type in Types(assembly.MainModule.Types).Where(t=>Regex.IsMatch(t.FullName,args[1],RegexOptions.IgnoreCase))) {
 if(!type.Methods.Any(m=>Regex.IsMatch(m.Name,args[2],RegexOptions.IgnoreCase))) continue;
 Console.WriteLine("TYPE "+type.FullName);
 foreach(var method in type.Methods.Where(m=>Regex.IsMatch(m.Name,args[2],RegexOptions.IgnoreCase))) {
  Console.WriteLine(method.FullName);
  if(args.Contains("--il") && method.HasBody) foreach(var instruction in method.Body.Instructions) Console.WriteLine("  "+instruction);
 }
}
