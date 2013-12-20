layout(local_size_x = 4, local_size_y = 4, local_size_z = 4) in;

layout(binding = GLOBAL_VERTEX_DATA) uniform GlobalVertexData
{
   mat4 vp;
   mat4 view;
   mat4 view_nt;
   mat4 proj;
   mat4 inv_vp;
   mat4 inv_view;
   mat4 inv_view_nt;
   mat4 inv_proj;
   vec4 camera_pos;
   vec4 frustum[6];
} global_vert;

layout(binding = 0, offset = 4) uniform atomic_uint lod0_cnt; // Outputs to instance variable.
//layout(binding = 0, offset = 24) uniform atomic_uint lod1_cnt;

layout(binding = 0) buffer SourceData
{
   readonly vec4 pos[];
} source_data;

layout(binding = 1) buffer DestData
{
   writeonly vec4 pos[];
} culled;

#if 0
uint work_size()
{
   return gl_WorkGroupSize.x *
      gl_WorkGroupSize.y *
      gl_WorkGroupSize.z *
      gl_NumWorkGroups.x *
      gl_NumWorkGroups.y *
      gl_NumWorkGroups.z;
}
#endif

uint get_invocation()
{
   uint work_group = gl_WorkGroupID.x * gl_NumWorkGroups.y * gl_NumWorkGroups.z + gl_WorkGroupID.y * gl_NumWorkGroups.z + gl_WorkGroupID.z;
   return work_group * gl_WorkGroupSize.x * gl_WorkGroupSize.y * gl_WorkGroupSize.z + gl_LocalInvocationIndex;
}

void main()
{
   vec4 point = source_data.pos[get_invocation()];
   vec4 pos = vec4(point.xyz, 1.0);

#if 0
   float depth = dot(pos, global_vert.frustum[0]);
   if (depth < -point.w) // Culled
      return;

   for (int i = 1; i < 6; i++)
      if (dot(pos, global_vert.frustum[i]) < -point.w) // Culled
         return;

   if (depth > 100.0) // LOD1
   {
      uint counter = atomicCounterIncrement(lod1_cnt);
      culled.pos[counter] = point;
   }
   else
   {
      uint counter = atomicCounterIncrement(lod0_cnt);
      uint offset = work_size();
      culled.pos[counter + offset] = point;
   }
#endif

   for (int i = 0; i < 6; i++)
      if (dot(pos, global_vert.frustum[i]) < -point.w) // Culled
         return;

   uint counter = atomicCounterIncrement(lod0_cnt);
   culled.pos[counter] = point;
}

