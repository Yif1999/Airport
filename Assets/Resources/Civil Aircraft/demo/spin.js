function Update()
{
transform.RotateAround(transform.position, transform.forward, Time.deltaTime * -360f);
}