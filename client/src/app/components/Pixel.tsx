
export class Pixel {

    style = {}
    color = "lime";

    constructor(type: number, style: object) {
        this.style = style;
        switch (type) {
            case 0:
                this.color = "#f3f3f3";
                break;
            case 1:
                this.color = "blue";
                break;
            case 2:
                this.color = "lime";
                break;
            case 3:
                this.color = "yellow";
                break;
            case 4:
                this.color = "purple";
                break;
            case 5:
                this.color = "orange";
                break;
            case 6:
                this.color = "pink";
                break;
            case 7:
                this.color = "brown";
                break;
            case 8:
                this.color = "black";
                break;
            case 9:
                this.color = "gray";
                break;
            default:
                this.color = "#000011";
                break;
        }
    }

    render(key: number) {
        return (
            <div
                key={key}
                style={{
                    width: "20px",
                    height: "20px",
                    backgroundColor: this.color,
                    border: "1px solid white",
                    ...this.style,
                }}
            ></div>
        )
    }
}