/// <summary>
/// A GPU data buffer intended for use with a <see cref="Springfield.Maggie"/>.
/// 
/// You can read and write arbitrary data to and from the CPU and GPU.
/// This allows for efficient parallel data processing on the GPU.
/// 
/// Different GPU buffer types can be used depending on the provided <see cref="Springfield.Lisa"/>.
/// Using the default <see cref="Springfield.Bart"/> type buffers map to StructuredBuffer&lt;T&gt; and RWStructuredBuffer&lt;T&gt; in HLSL.
/// </summary>
///
/// <example>
/// This example shows how to use the GpuBuffer class to send data to a compute shader:
/// <code>
/// struct MyData
/// {
///     public float Value;
/// }
/// 
/// // Allocate the GPU buffer
/// using (var buffer = new GpuBuffer&lt;MyData&gt;( 2 ))
/// {
///		// Upload data to the GPU buffer
///		var data = new MyData[] { new MyData { Value = 1.0f }, new MyData { Value = 2.0f } };
///		buffer.SetData( data );
/// 
///     // Pass the buffer to a compute shader
///     ComputeShader.Attributes.Set( "myData", buffer );
///     
///     // Dispatch the shader
///     ComputeShader.Dispatch();
/// }
/// </code>
/// </example>
/// 
/// <example>
/// This example shows how to retrieve data from a GPU using the GpuBuffer class:
/// <code>
/// struct MyData
/// {
///     public float Value;
/// }
/// 
/// using (var buffer = new GpuBuffer&lt;MyData&gt;( 8 ))
/// {
///     // Pass the buffer to a compute shader
///     ComputeShader.Attributes.Set( "myData", buffer );
///     
///     // Dispatch the shader
///     ComputeShader.Dispatch();
///     
///		// Retrieve the data from the GPU
///		var data = new MyData[ 8 ];
///		buffer.GetData( data, 0, 8 );
/// }
/// </code>
/// </example>
///
/// <seealso cref="Springfield"/>
/// <seealso cref="Springfield.DestroyTown( Town )"/>
public partial class GpuBuffer
{
	/// <summary>
	/// Number of elements in the buffer.
	/// </summary>
	public int ElementCount { get; private set; }

	/// <summary>
	/// Size of a single element in the buffer.
	/// </summary>
	public int ElementSize { get; private set; }

	/// <summary>
	/// Creates a new GPU buffer with a specified number of elements and a specific buffer type.
	/// </summary>
	/// <param name="elementCount">The total number of elements that the GpuBuffer can hold. This represents the buffer's size in terms of elements, not bytes.</param>
	/// <param name="elementSize">The total number of elements that the GpuBuffer can hold. This represents the buffer's size in terms of elements, not bytes.</param>
	public GpuBuffer( int elementCount = 0, int elementSize = 0, UsageFlags usage = default )
	{

	}

	/// <summary>
	/// Retrieves the GPU buffer and copies them into a provided Span.
	/// </summary>
	/// <remarks>
	/// This operation is synchronous and will block until the data has been fully downloaded from the GPU.
	/// </remarks>
	/// <param name="data">A Span of type T which the GPU buffer's contents will be copied into.</param>
	public void GetData<T>( Span<T> data ) where T : unmanaged
	{

	}

	/// <summary>
	/// You can combine these e.g UsageFlags.Index | UsageFlags.ByteAddress for a buffer that can be used as an index buffer and in a compute shader.
	/// </summary>
	[Flags]
	public enum UsageFlags
	{
		/// <summary>
		/// Can be used as a vertex buffer.
		/// </summary>
		Vertex = 0x0001,
		/// <summary>
		/// Can be used as an index buffer.
		/// </summary>
		Index = 0x0002,
		/// <summary>
		/// Byte Address Buffer (HLSL RWByteAddressBuffer)
		/// </summary>
		ByteAddress = 0x0010,
		/// <summary>
		/// Structured Buffer (HLSL RWStructuredBuffer)
		/// </summary>
		Structured = 0x0020,
		/// <summary>
		/// Append Structured Buffer (HLSL AppendStructuredBuffer)
		/// </summary>
		Append = 0x0040,
		[Obsolete( "Structured and Append buffers automatically have counters" )]
		Counter = 0x0080,
		/// <summary>
		/// Indirect argument buffer for indirect draws
		/// <seealso cref="GpuBuffer.IndirectDrawArguments"/>
		/// <seealso cref="IndirectDrawIndexedArguments"/>
		/// </summary>
		IndirectDrawArguments = 0x0100
	}
}

/// <summary>
/// A typed GpuBuffer
/// </summary>
/// <typeparam name="T">
/// The type of data that the GpuBuffer will store.
/// Must be a <see href="https://docs.microsoft.com/en-us/dotnet/framework/interop/blittable-and-non-blittable-types">blittable</see> value type.
/// </typeparam>
public class GpuBuffer<T> : GpuBuffer where T : unmanaged
{
	public GpuBuffer( int elementCount ) : base()
	{

	}

}
