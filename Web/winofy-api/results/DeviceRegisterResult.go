package results

const (
	DeviceRegisterOk = "Ok"
	DeviceRegisterUnauthorized = "Unauthorized"
	DeviceRegisterDeviceExists = "DeviceExists"
	DeviceRegisterUnknown = "Unknown"
)

type DeviceRegisterResult struct {
	Success bool `json:"success"`
	Message string `json:"message"`
}
