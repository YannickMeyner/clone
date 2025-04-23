import styles from "./keyboardLabel.module.css";

export default function KeyboardLabel({ label, description }: { label: string, description?: string }) {

    return (
        <div className={styles.container}>
            <div className={styles.key}>
                {label}
            </div>
            <div className={styles.description}>
                {description}
            </div>
        </div>
    );
}