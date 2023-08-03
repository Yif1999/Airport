var turnSpeed = 50;

function Update()
{
 var eang = gameObject.transform.localEulerAngles;
 if(Input.GetKey(KeyCode.Z))
 {
 eang.y -= turnSpeed * Time.deltaTime;
  //transform.RotateAround(transform.position, transform.up, Time.deltaTime * turnSpeed);
 }
 if(Input.GetKey(KeyCode.X))
 {
 eang.y += turnSpeed * Time.deltaTime;
  //transform.RotateAround(transform.position, transform.up, Time.deltaTime * -turnSpeed);
 }
 //eang.z = 0;
 //eang.x = 0;
 if(eang.y > 180) eang.y -= 360;
  if(eang.y > 30) eang.y = 30;
else if(eang.y < -30) eang.y = -30;
if(eang.y < 0) eang.y += 360; 
 gameObject.transform.localEulerAngles = eang;

}