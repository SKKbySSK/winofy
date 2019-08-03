package devices

import (
	"github.com/emicklei/go-restful"
	"../results"
	"strconv"
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

	notification := request.QueryParameter("notifications")
	checkNotification := false

	if len(notification) > 0 {
		checkNotification, err = strconv.ParseBool(notification)

		if err != nil {
			writeJsonResponse(response, new(results.ListDevicesResult), 400)
			return
		}
	}

	q := "SELECT DeviceId, Name, Description FROM Devices WHERE Username = ?"
	rows, err := sqlConnection.Query(q, *username)
	defer rows.Close()

	if err != nil {
		writeJsonResponse(response, new(results.ListDevicesResult), 500)
		return
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

		device = &results.Device{
			Id: id,
			Name: name,
			Description: desc,
		}
		devices = append(devices, *device)
	}

	res := new(results.ListDevicesResult)
	res.Devices = devices

	if checkNotification {
		q = "SELECT DeviceId, Notification FROM Devices WHERE Username = ?"
		rows, err = sqlConnection.Query(q, *username)
		defer rows.Close()

		if err != nil {
			writeJsonResponse(response, new(results.ListDevicesResult), 500)
			return
		}

		var notification bool
		for rows.Next() {
			rows.Scan(&id, &notification)

			for i, d := range res.Devices {
				if d.Id == id {
					res.Devices[i].Notification = &notification
					break
				}
			}
		}
	}

	writeJsonResponse(response, res, 200)
}