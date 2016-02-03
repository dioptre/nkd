<Query Kind="Expression">
  <Connection>
    <ID>432999a6-f364-4450-8431-bbdce1c13b67</ID>
    <Persist>true</Persist>
    <Driver>EntityFramework</Driver>
    <Server>nkddb</Server>
    <CustomAssemblyPath>C:\expedit\projects\nkd\src\main\NKD.Module.BusinessObjects\bin\Debug\NKD.Module.BusinessObjects.dll</CustomAssemblyPath>
    <CustomTypeName>NKD.Module.BusinessObjects.NKDC</CustomTypeName>
    <CustomMetadataPath>res://NKD.Module.BusinessObjects/NKD.csdl|res://NKD.Module.BusinessObjects/NKD.ssdl|res://NKD.Module.BusinessObjects/NKD.msl</CustomMetadataPath>
    <SqlSecurity>true</SqlSecurity>
    <Database>NKD</Database>
    <UserName>nkd_user1</UserName>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAnGvDT5nkcki3ENvFWVbM4wAAAAACAAAAAAAQZgAAAAEAACAAAAABmIBJDjxrx6swbrZccrr8by9jPcjQwTAwnYvBQyNYrAAAAAAOgAAAAAIAACAAAAD4ZYdHLqGfol0bKvkHyvvusveE2WUq3WpTQS86bgXcaBAAAABTfhVXrqwQRlGqO45TlMS6QAAAAOnuy7vyzbklylfk4Z+37sITQOi8Tu0GFFyROSLzbUKY0KtPuqY/r3m3Ke22gdfKZce+wBbhUcWsO3lww9wKw4E=</Password>
  </Connection>
</Query>


 
from o in GraphDataRelation.Where(f => f.GraphDataGroupID == new Guid("E63D3697-3B5A-4F38-9099-C401DA49A719"))
                                                             join q in GraphDataRelation.Where(f=>f.GraphDataGroupID == new Guid("E63D3697-3B5A-4F38-9099-C401DA49A719"))
                                                                on o.FromGraphDataID equals q.ToGraphDataID into oq
                                                             from soq in oq.DefaultIfEmpty()                        
                                                             where soq == null
                                                             select o.FromGraphDataID