package devices

import (
	"github.com/emicklei/go-restful"
	"../results"
)

type ListRouting struct {

}

func (list *ListRouting) Route(ws *restful.WebService) {
	ws.Route(ws.GET("devices/list").To(list.registerFunc))
}

func (list *ListRouting) registerFunc(request *restful.Request, response *restful.Response) {
	if !isAuthorized(request) {
		writeJsonResponse(response, new(results.ListDevicesResult), 401)
		return
	}

	username, err := getUsernameFromToken(request.HeaderParameter("token"))
	if err != nil {
		writeJsonResponse(response, new(results.ListDevicesResult), 400)
		return
	}

	q := "SELECT DeviceId, Name, Description FROM Devices WHERE Username = ?"
	rows, err := sqlConnection.Query(q, *username)

	if err != nil {
		writeJsonResponse(response, new(results.ListDevicesResult), 500)
	}

	var (
		id string
		name string
		desc string
		device *results.Device
		devices []results.Device
	)

	for rows.Next() {
		rows.Scan(&id, &name, &desc)

		device = new(results.Device)
		device.Id = id
		device.Name = name
		device.Description = desc
		devices = append(devices, *device)
	}

	res := new(results.ListDevicesResult)
	res.Devices = devices

	writeJsonResponse(response, res, 200)
}