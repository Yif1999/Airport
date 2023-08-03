var turnSpeed = 50;

function Update()
{
 var eang = gameObject.transform.localEulerAngles;
 if(Input.GetKey(KeyCode.Z))
 {
 eang.x -= turnSpeed * Time.deltaTime;
  //transform.RotateAround(transform.position, transform.up, Time.deltaTime * turnSpeed);
 }
 if(Input.GetKey(KeyCode.X))
 {
 eang.x += turnSpeed * Time.deltaTime;
  //transform.RotateAround(transform.position, transform.up, Time.deltaTime * -turnSpeed);
 }
 //eang.z = 0;
 //eang.x = 0;
 if(eang.x > 180) eang.x -= 360;
  if(eang.x > 30) eang.x = 30;
else if(eang.x < -30) eang.x = -30;
if(eang.x < 0) eang.x += 360; 
 gameObject.transform.localEulerAngles = eang;

}