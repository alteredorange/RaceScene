using UnityEngine;
using UnityEditor;

using SplineEditorExtensions;

public partial class SplineEditor : Editor
{
	private GUIStyle sceneGUIStyle = null;
	private GUIStyle sceneGUIStyleToolLabel = null;
	
	private float toolSphereAlpha = 0f;
	private float toolSphereSize = 1f;
	
	private float lastRealTime = 0f;
	private float deltaTime = 0f;
	
	public void OnSceneGUI( )
	{
		Spline spline = target as Spline;
	
		InitSceneGUIStyles( );
		
		DrawSplineLabels( spline );
		DrawHandles( spline );
		
		HandleMouseInput( spline );
		
		if( GUI.changed )
			ApplyChangesToTarget( target );
	}
	
	private void HandleMouseInput( Spline spline )
	{
		CalcDeltaTime( );
		
		if( !EditorGUI.actionKey )
		{
			toolSphereSize = 10f;
			toolSphereAlpha = 0f;
			
			return;
		}
		
		Ray mouseRay = Camera.current.ScreenPointToRay( new Vector2( Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y - 32f ) );
		
		float splineParam = spline.GetClosestPointParamToRay( mouseRay, 3 );
		
		Vector3 position = spline.GetPositionOnSpline( splineParam );
		
		float currentDistance = Vector3.Cross( mouseRay.direction, position - mouseRay.origin ).magnitude;
		
		SplineNode selectedNode = null;
		
		foreach( SplineNode node in spline.SplineNodes )
		{
			float newDistance = Vector3.Distance( node.Position, position );
			
			if( newDistance < currentDistance || newDistance < 0.2f * HandleUtility.GetHandleSize( node.Position ) )
			{
				currentDistance = newDistance; 
				
				selectedNode = node;
			}
		}
		
		toolSphereAlpha = Mathf.Lerp( toolSphereAlpha, 0.75f, deltaTime * 4f );
		
		if( selectedNode != null )
		{
			position = selectedNode.Position;
			
			Handles.color = new Color( .7f, 0.15f, 0.1f, toolSphereAlpha );
			Handles.SphereCap( 0, position, Quaternion.identity, HandleUtility.GetHandleSize( position ) * 0.25f * toolSphereSize );
			Handles.color = Color.white;
			
			Handles.Label( position - Camera.current.transform.up * HandleUtility.GetHandleSize( position ) * 0.3f, "Delete Node (" + selectedNode.gameObject.name + ")", sceneGUIStyleToolLabel ); 
			
			toolSphereSize = Mathf.Lerp( toolSphereSize, 1.3f + Mathf.Sin( Time.realtimeSinceStartup * 2f ) * 0.1f, deltaTime * 15f );
		}
		else
		{
			Handles.color = new Color( 1f, 0.5f, 0f, toolSphereAlpha );
			Handles.SphereCap( 0, position, Quaternion.identity, HandleUtility.GetHandleSize( position ) * 0.25f * toolSphereSize );
			Handles.color = Color.white;
			
			Handles.Label( position - Camera.current.transform.up * HandleUtility.GetHandleSize( position ) * 0.3f, "Insert Node", sceneGUIStyleToolLabel ); 
			
			toolSphereSize = Mathf.Lerp( toolSphereSize, .8f + Mathf.Sin( Time.realtimeSinceStartup * 2f ) * 0.1f, deltaTime * 15f );
		}
		
		if( Event.current.type == EventType.mouseDown && Event.current.button == 1 )
		{
			Undo.RegisterSceneUndo( ( selectedNode != null ) ? "Delete Spline Node (" + selectedNode.name +  ")" : "Insert Spline Node" );
			
			if( selectedNode != null )
				DestroyImmediate( selectedNode.gameObject );
			else
				InsertNode( spline, splineParam );
			
			ApplyChangesToTarget( spline );
		}
		
		
		HandleUtility.Repaint( );
	}
	
	private void DrawHandles( Spline spline )
	{
		Handles.lighting = true;
		
		foreach( SplineNode node in spline.SplineNodes )
		{
			if( DrawNodeLabels( node ) )
				continue;
			
			if( Tools.current == Tool.None || Tools.current == Tool.View )
				continue;
			
			switch( Tools.current )
			{
			case Tool.Rotate:
				Undo.SetSnapshotTarget( node.transform, "Rotate Spline Node: " + node.name );
				
				Quaternion newRotation = Handles.RotationHandle( GetRotation( node ), node.transform.position );
				
				Handles.color = Color.blue;
				Handles.ArrowCap( 0, node.transform.position, Quaternion.LookRotation( node.transform.forward ), HandleUtility.GetHandleSize( node.transform.position ) * 0.5f );
				Handles.color = Color.white;
				
				if( !GUI.changed )
					break;
				
				SetRotation( node, newRotation );
				
				EditorUtility.SetDirty( target );
				
				break;
				
			case Tool.Move:
			case Tool.Scale: 
				Undo.SetSnapshotTarget( node.transform, "Move Spline Node: " + node.name );
				
				Vector3 newPosition = Handles.PositionHandle( node.transform.position, GetRotation( node ) );
				
				if( !GUI.changed )
					break;
				
				node.transform.position = newPosition;
				
				EditorUtility.SetDirty( target );
				
				break;
			}
			
			CreateSnapshot( );
		}
	}
	
	private void SetRotation( SplineNode splineNode, Quaternion rotation )
	{
		if( Tools.pivotRotation == PivotRotation.Global )
			splineNode.transform.rotation = rotation;
		else
			splineNode.transform.localRotation = rotation;
	}
	
	private Quaternion GetRotation( SplineNode splineNode )
	{
		if( Tools.pivotRotation == PivotRotation.Global )
			return splineNode.transform.rotation;
		else
			return splineNode.transform.localRotation;
	}
	
	private void DrawSplineLabels( Spline spline )
	{
		if( Event.current.alt && Event.current.shift )
		{
			foreach( SplineSegment item in spline.SplineSegments )
			{
				Vector3 positionOnSpline = spline.GetPositionOnSpline( item.ConvertSegmentToSplineParamter( .5f ) );
				
				Handles.Label( positionOnSpline + Camera.current.transform.up * HandleUtility.GetHandleSize( positionOnSpline ) * 0.2f, item.Length.ToString( ), sceneGUIStyle );
			}
			
			Handles.Label( spline.transform.position + Camera.current.transform.up * .3f * HandleUtility.GetHandleSize( spline.transform.position ), "Length: " + spline.Length.ToString( ), sceneGUIStyle );
				
			return;
		}
	}
	
	private bool DrawNodeLabels( SplineNode splineNode )
	{
		if( Event.current.alt && !Event.current.shift )
			Handles.Label( splineNode.transform.position + Camera.current.transform.up * HandleUtility.GetHandleSize( splineNode.transform.position ) * 0.2f, splineNode.name, sceneGUIStyle );
		
		return Event.current.alt || EditorGUI.actionKey;
	}
	
	private void CreateSnapshot( )
	{
		if( Input.GetMouseButtonDown( 0 ) ) 
		{
			Undo.CreateSnapshot( );
			Undo.RegisterSnapshot( );
		}
	}
	
	private void InitSceneGUIStyles( )
	{
		if( sceneGUIStyle == null )
		{	
			sceneGUIStyle = new GUIStyle( EditorStyles.miniTextField );
			sceneGUIStyle.alignment = TextAnchor.MiddleCenter;
		}
		
		if( sceneGUIStyleToolLabel == null )
		{	
			sceneGUIStyleToolLabel = new GUIStyle( EditorStyles.textField );
			sceneGUIStyleToolLabel.alignment = TextAnchor.MiddleCenter;
			sceneGUIStyleToolLabel.padding = new RectOffset( -8, -8, -2, 0 );
			sceneGUIStyleToolLabel.fontSize = 20;
		}
	}
	
	private void InsertNode( Spline spline, float splineParam )
	{
		SplineNode splineNode = CreateSplineNode( "New Node", spline.GetPositionOnSpline( splineParam ), spline.GetOrientationOnSpline( splineParam ), spline.transform );
		SplineSegment segment = spline.GetSplineSegment( splineParam );
		
		int startNodeIndex = spline.splineNodesArray.IndexOf( segment.StartNode );
		
		spline.splineNodesArray.Insert( startNodeIndex + 1, splineNode );
	}
	
	private SplineNode CreateSplineNode( string nodeName, Vector3 position, Quaternion rotation, Transform parent )
	{
		GameObject gameObject= new GameObject( nodeName );
		
		gameObject.transform.position = position;
		gameObject.transform.rotation = rotation;
		
		gameObject.transform.parent = parent;
		
		SplineNode splineNode = gameObject.AddComponent<SplineNode>( );
		
		return splineNode;
	}
	
	private void CalcDeltaTime( )
	{	
		deltaTime = Time.realtimeSinceStartup - lastRealTime;
		lastRealTime = Time.realtimeSinceStartup;
		
		if( deltaTime > 0.1f )
			deltaTime = 0f;
	}
}
