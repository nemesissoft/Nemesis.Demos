// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Blocker Bug", "S2190:Loops and recursions should not be infinite", Justification = "Loop is ended by program termination", Scope = "member", Target = "~M:Nemesis.Demos.DemoRunner.Run(System.String[])~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Major Code Smell", "S6966:Awaitable method should be used", Justification = "Demo runnable has 2 Run methods on purpose", Scope = "member", Target = "~M:Nemesis.Demos.DemoRunner.Run(System.String[])~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "<Pending>", Scope = "member", Target = "~M:Nemesis.Demos.Runnable.GetMethodInfo(System.String,System.Object)~System.Reflection.MethodInfo")]
[assembly: SuppressMessage("Minor Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "<Pending>", Scope = "member", Target = "~M:Nemesis.Demos.DemoRunner.Run(System.String[])~System.Threading.Tasks.Task")]
