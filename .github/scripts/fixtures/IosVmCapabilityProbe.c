#include <Hypervisor/Hypervisor.h>
#include <stdio.h>

int main(void)
{
	hv_return_t create_result = hv_vm_create(NULL);
	printf("create=%d\n", (int)create_result);

	if (create_result != HV_SUCCESS)
	{
		printf("destroy=not-attempted\n");
		return 20;
	}

	hv_return_t destroy_result = hv_vm_destroy();
	printf("destroy=%d\n", (int)destroy_result);
	return destroy_result == HV_SUCCESS ? 0 : 21;
}
