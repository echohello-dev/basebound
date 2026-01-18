using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using SandboxModelContextProtocol.Editor.Commands.Attributes;

namespace SandboxModelContextProtocol.Editor.Tools;

[McpEditorToolType]
public class EditorSceneTool
{
	[McpEditorTool]
	public static JsonObject GetActiveEditorScene()
	{
		Scene? scene = SceneEditorSession.Active.Scene;
		if (scene == null)
		{
			return new JsonObject(null);
		}

		return scene.Serialize();
	}

	[McpEditorTool]
	public static async Task LoadEditorSceneFromPath(string path)
	{
		// Validate input
		if (string.IsNullOrWhiteSpace(path))
		{
			throw new ArgumentException("Scene path cannot be null or empty", nameof(path));
		}

		// Try multiple path formats
		string[] pathVariants = {
			path,
			path.Replace( '\\', '/' ),
			$"Assets/scenes/{System.IO.Path.GetFileName( path )}",
			System.IO.Path.GetFileName( path ).Replace( ".scene", "" )
		};

		SceneFile? sceneFile = null;
		foreach (string pathVariant in pathVariants)
		{
			try
			{
				if (ResourceLibrary.TryGet(pathVariant, out sceneFile))
					break;
			}
			catch (Exception ex)
			{
				Log.Warning($"Failed to load scene from path '{pathVariant}': {ex.Message}");
			}
		}

		if (sceneFile == null)
		{
			throw new InvalidOperationException($"Scene file not found. Tried paths: {string.Join(", ", pathVariants)}");
		}

		try
		{
			await InvokeEditorSceneLoad(sceneFile, path);
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException($"Failed to load scene from '{path}': {ex.Message}", ex);
		}
	}

	private static async Task InvokeEditorSceneLoad(SceneFile sceneFile, string path)
	{
		var editorSceneType = typeof(EditorScene);
		var loadMethod = editorSceneType
			.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.FirstOrDefault(method =>
			{
				if (!method.Name.Contains("Load", StringComparison.OrdinalIgnoreCase))
					return false;

				var parameters = method.GetParameters();
				return parameters.Length == 1 && parameters[0].ParameterType == typeof(SceneFile);
			});

		if (loadMethod == null)
		{
			loadMethod = editorSceneType
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.FirstOrDefault(method =>
				{
					if (!method.Name.Contains("Load", StringComparison.OrdinalIgnoreCase))
						return false;

					var parameters = method.GetParameters();
					return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
				});
		}

		if (loadMethod == null)
		{
			throw new InvalidOperationException("EditorScene does not expose a compatible Load method in this S&box version.");
		}

		object result;
		try
		{
			result = loadMethod.GetParameters()[0].ParameterType == typeof(SceneFile)
				? loadMethod.Invoke(null, new object[] { sceneFile })
				: loadMethod.Invoke(null, new object[] { path });
		}
		catch (TargetInvocationException ex)
		{
			throw ex.InnerException ?? ex;
		}

		if (result is Task task)
		{
			await task;
			return;
		}

		if (result is ValueTask valueTask)
		{
			await valueTask.AsTask();
		}
	}

	[McpEditorTool]
	public static void SaveAllEditorSessions()
	{
		EditorScene.SaveAllSessions();
	}

	[McpEditorTool]
	public static void SaveActiveEditorSession()
	{
		EditorScene.SaveSession();
	}

	[McpEditorTool]
	public static JsonArray GetAllEditorSessions()
	{
		return new JsonArray(SceneEditorSession.All.Select(s => s.Scene.Serialize()).ToArray());
	}

	[McpEditorTool]
	public static JsonObject GetActiveEditorSession()
	{
		var activeSession = SceneEditorSession.Active;
		if (activeSession?.Scene == null)
		{
			throw new InvalidOperationException("No active editor session found");
		}

		return activeSession.Scene.Serialize();
	}
}
