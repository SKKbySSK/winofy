package devices

import "github.com/emicklei/go-restful"

type DeviceRouting struct {

}

func (device *DeviceRouting) Route(ws *restful.WebService) {
	ws.Route(ws.GET("devices/device").To(device.registerFunc))
}

func (device *DeviceRouting) registerFunc(request *restful.Request, response *restful.Response) {

}
